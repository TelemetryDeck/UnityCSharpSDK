using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.Networking;

namespace TelemetryClient
{
    using TelemetrySignalType = String;

    internal class SignalManager : MonoBehaviour
    {
        private const float MINIMUM_WAIT_TIME_BETWEEN_REQUESTS = 10; // seconds

        private SignalCache<SignalPostBody> signalCache;
        private TelemetryManagerConfiguration configuration;
        private Coroutine sendCoroutine = null;

        public static SignalManager CreateSignalManager(TelemetryManagerConfiguration configuration)
        {
            var gameObject = new GameObject("TelemetrySignalManager");
            DontDestroyOnLoad(gameObject);
            var @this = gameObject.AddComponent<SignalManager>();
            @this.configuration = configuration;

            // We automatically load any old signals from disk on initialisation
            @this.signalCache = new SignalCache<SignalPostBody>(showDebugLogs: configuration.showDebugLogs);

            @this.StartTimer();
            return @this;
        }

        /// <summary>
        /// Setup a timer to send the Signals
        /// </summary>
        private void StartTimer()
        {
            if (sendCoroutine != null)
            {
                StopCoroutine(sendCoroutine);
                sendCoroutine = null;
            }
            IEnumerator SendSignals()
            {
                while (true)
                {
                    // Fire the signal immediately to attempt to send any cached Signals from a previous session
                    CheckForSignalsAndSend();
                    yield return new WaitForSeconds(MINIMUM_WAIT_TIME_BETWEEN_REQUESTS);
                }
            }
            sendCoroutine = StartCoroutine(SendSignals());
        }

        /// <summary>
        /// Adds a signal to the process queue
        /// </summary>
        internal void ProcessSignal(TelemetryManagerConfiguration configuration, TelemetrySignalType signalType, string clientUser = null, Dictionary<string, string> additionalPayload = null)
        {
            var payload = SignalPayload.GetCommonPayload();
            payload.additionalPayload = additionalPayload;

            string userIdentifier = clientUser ?? DefaultUserIdentifier;
            // calculate SHA256 hash async
            var job = new CreateUserHashJob()
            {
                userIdentifier = new NativeArray<char>(userIdentifier.ToCharArray(), Allocator.Persistent),
                userHash = new NativeArray<char>(CreateUserHashJob.UserHashStringLength, Allocator.Persistent)
            };
            IEnumerator WaitForJob()
            {
                try
                {
                    JobHandle handle = job.Schedule();

                    while (true)
                    {
                        if (handle.IsCompleted)
                            break;
                        yield return new WaitForEndOfFrame();
                    }
                    handle.Complete();

                    var signalPostBody = new SignalPostBody()
                    {
                        type = signalType,
                        appID = new Guid(configuration.TelemetryAppID),
                        clientUser = new string(job.userHash.ToArray()),
                        payload = payload.ToMultiValueDimension(),
                        receivedAt = DateTime.Now,
                        sessionID = configuration.SessionId.ToString()
                    };

                    if (configuration.showDebugLogs)
                        Debug.Log($"Adding signal to cache: {signalPostBody.type}");

                    signalCache.Push(signalPostBody);
                }
                finally
                {
                    job.userIdentifier.Dispose();
                    job.userHash.Dispose();
                }
            }
            StartCoroutine(WaitForJob());
        }

        /// <summary>
        /// Send signals once we have more than the minimum.
        /// If any fail to send, we put them back into the cache to send later.
        /// </summary>
        private void CheckForSignalsAndSend()
        {
            if (configuration.showDebugLogs)
            {
                Debug.Log($"Current signal cache count: {signalCache.Count}");
            }

            var queuedSignals = signalCache.Pop();
            if (queuedSignals.Count > 0)
            {
                if (configuration.showDebugLogs)
                {
                    Debug.Log($"Sending {queuedSignals.Count} signals leaving a cache of {signalCache.Count} signals");
                }

                Send(queuedSignals, completion: (data, response, error) =>
                {
                    if (error != null)
                    {
                        if (configuration.showDebugLogs)
                        {
                            Debug.LogError($"Failed to send signal data:\n{data}");
                            Debug.LogError(error);
                        }
                        // The send failed, put the signal back into the queue
                        signalCache.Push(queuedSignals);
                        return;
                    }

                    // Check for valid status code response
                    if (!string.IsNullOrEmpty(error))
                    {
                        if (configuration.showDebugLogs)
                        {
                            Debug.LogError(error);
                        }
                        // The send failed, put the signal back into the queue
                        signalCache.Push(queuedSignals);
                        return;
                    }
                    else if (data != null)
                    {
                        if (configuration.showDebugLogs)
                        {
                            Debug.Log(data);
                        }
                    }
                });
            }
        }

        /// <summary>
        /// Before the app terminates, we want to save any pending signals to disk
        /// </summary>
        private void OnDestroy()
        {
            if (configuration.showDebugLogs)
            {
                Debug.Log("App will terminate");
            }

            signalCache.BackupCache();
        }

        private void Send(List<SignalPostBody> signalPostBodies, Action<string, int, string> completion)
        {
            var stringBuilder = new StringBuilder(capacity: 96, maxCapacity: 100);
            stringBuilder.Append(configuration.ApiBaseUrl);
            stringBuilder.Append("/api/v1/apps/");
            stringBuilder.Append(configuration.TelemetryAppID);
            stringBuilder.Append("/signals/multiple/");
            string url = stringBuilder.ToString();

            string data = JsonConvert.SerializeObject(signalPostBodies);
            byte[] bytes = Encoding.UTF8.GetBytes(data);
            if (configuration.showDebugLogs)
                Debug.Log(data);
            var webRequest = new UnityWebRequest();
            webRequest.url = url;
            webRequest.method = UnityWebRequest.kHttpVerbPOST;
            webRequest.uploadHandler = new UploadHandlerRaw(bytes);

            webRequest.SetRequestHeader("Content-Type", "application/json");

            UnityWebRequestAsyncOperation asyncOperation = webRequest.SendWebRequest();
            asyncOperation.completed += (operation) =>
            {
                switch (webRequest.result)
                {
                    case UnityWebRequest.Result.ConnectionError:
                    case UnityWebRequest.Result.DataProcessingError:
                    case UnityWebRequest.Result.ProtocolError:
                        completion(data, -1, webRequest.error);
                        break;
                    case UnityWebRequest.Result.Success:
                        completion(data, 0, null);
                        break;
                }
            };
        }

        #region Helpers
        /// <summary>
        /// The default user identifier. If the platform supports it, the identifierForVendor. Otherwise, system version
        /// and build number (in which case it's strongly recommended to supply an email or UUID or similar identifier for
        /// your user yourself.
        /// </summary>
        public string DefaultUserIdentifier
        {
            get
            {
                if (configuration.defaultUser != null)
                {
                    return configuration.defaultUser;
                }
                else
                {
                    return "TODO";// UIDevice.current.identifierForVendor?.uuidString ?? "unknown user \(SignalPayload.systemVersion) \(SignalPayload.buildNumber)";
                }
                // TODO get uuid
                // else the following
                /*
            #if DEBUG
                Debug.LogWarning("[Telemetry] On this platform, Telemetry can't generate a unique user identifier. It is recommended you supply one yourself. More info: https://telemetrydeck.com/pages/signal-reference.html")
            #else
            return "unknown user \(SignalPayload.platform) \(SignalPayload.systemVersion) \(SignalPayload.buildNumber)"
            #endif
                */
            }
        }
        #endregion

        private struct TelemetryServerError
        {
            public enum EKind
            {
                Unknown, Unauthorised, Forbidden, PayloadTooLarge, InvalidStatusCode
            }

            public EKind kind;
            public int? statusCode;

            public override string ToString()
            {
                switch (kind)
                {
                    case EKind.InvalidStatusCode:
                        return $"Invalid status code {statusCode ?? -1}";
                    case EKind.Unauthorised:
                        return "Unauthorized (401)";
                    case EKind.Forbidden:
                        return "Forbidden (403)";
                    case EKind.PayloadTooLarge:
                        return "Payload is too large (413)";
                    default:
                        return "Unknown Error";
                }
            }
        }
    }
}
