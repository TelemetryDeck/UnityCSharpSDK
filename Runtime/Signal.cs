using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace TelemetryClient
{
    [Serializable]
    internal struct SignalPostBody
    {
        /// <summary>
        /// When was this signal generated
        /// </summary>
        public DateTime receivedAt;
        /// <summary>
        /// The App ID for this signal
        /// </summary>
        public Guid appID;
        /// <summary>
        /// A user identifier. This should be hashed on the client, and will be hashed + salted again
        /// on the server to break any connection to personally identifiable data.
        /// </summary>
        public string clientUser;
        /// <summary>
        /// A randomly generated session identifier. Should remain the same over the course of the session.
        /// </summary>
        public string sessionID;
        /// <summary>
        /// A name that describes the event that triggered the signal.
        /// </summary>
        public string type;
        /// <summary>
        /// Tags in the form "key:value" to be attached to the signal.
        /// </summary>
        public string[] payload;
        /// <summary>
        /// If <c>"true"</c>, marks the signal as a testing signal and
        /// shows it in a dedicated test mode UI in the Telemetry Viewer.
        /// If <c>"false"</c>, it is treated as a regular signal.
        /// </summary>
        public string isTestMode;
    }

    [Serializable]
    internal struct SignalPayload
    {
        public string platform;
        public string unityVersion;
        public string appVersion;
        public string isDebug;
        public string modelName;
        public string operatingSystem;
        public string operatingSystemFamily;
        public string scriptingBackend;
        public string locale;
        public string telemetryClientVersion;

        public Dictionary<string, string> additionalPayload;

        public static SignalPayload GetCommonPayload(Dictionary<string, string> additionalPayload = null)
        {
            return new SignalPayload
            {
                platform = CommonValues.Platform,
                unityVersion = CommonValues.UnityVersion,
                appVersion = CommonValues.AppVersion,
                isDebug = $"{CommonValues.IsDebug}",
                modelName = CommonValues.ModelName,
                operatingSystem = CommonValues.OperatingSystem,
                operatingSystemFamily = CommonValues.OperatingSystemFamily,
                scriptingBackend = CommonValues.ScriptingBackend,
                locale = CommonValues.Locale,
                telemetryClientVersion = TelemetryManager.TelemetryClientVersion,
                additionalPayload = additionalPayload
            };
        }

        /// Converts the object and `additionalPayload` to a `[String: String]` dictionary
        internal Dictionary<string, string> ToDictionary()
        {
            /// We need to convert the additionalPayload into new key/value pairs
            try
            {
                /// remove additional payload temporarily for serialization
                var _addPayload = additionalPayload;
                this.additionalPayload = null;
                /// Create a string-to-string Dictionary for the TelemetryDeck backend
                var jsonData = JsonConvert.SerializeObject(this);
                var dict = JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonData);
                /// Remove the empty additionalPayload sub dictionary
                dict.Remove("additionalPayload");
                this.additionalPayload = _addPayload;
                /// Add the additionalPayload as new key/value pairs
                if (additionalPayload != null)
                {
                    var merged = dict.Concat(additionalPayload)
                           .GroupBy(kv => kv.Key)
                           .ToDictionary(g => g.Key, g => g.First().Value);
                    return merged;
                }
                else
                {
                    return dict;
                }
            }
            catch (JsonSerializationException e)
            {
                Debug.LogError($"Failed to serialize Signal to string-Dictionary. {e}");
                return new Dictionary<string, string>();
            }
        }

        internal string[] ToMultiValueDimension()
        {
            return ToDictionary().Select(pair =>
            {
                return pair.Key.Replace(":", "_") + (":" + pair.Value);
            }).ToArray();
        }

    }

    static class CommonValues
    {
#if DEBUG
        public static bool IsDebug => true;
#else
            public static bool IsDebug => false;
#endif

        /// The operating system and its version
        public static string OperatingSystem => SystemInfo.operatingSystem;

        public static string AppVersion => Application.version;

        public static string UnityVersion => Application.unityVersion;

        /// The modelname as reported by SystemInfo.deviceModel
        public static string ModelName => SystemInfo.deviceModel;

        public static string OperatingSystemFamily => SystemInfo.operatingSystemFamily.ToString();

        /// The operating system as reported by Swift. Note that this will report catalyst apps and iOS apps running on
        /// macOS as "iOS". See `platform` for an alternative.
        /// See https://docs.unity3d.com/Manual/PlatformDependentCompilation.html
        public static string Platform => Application.platform.ToString();

        public static string ScriptingBackend
        {
            get
            {
#if ENABLE_MONO
                return "Mono";
#elif ENABLE_IL2CPP
                return "IL2CPP";
#else
                return "Unknown";
#endif
            }
        }

        /// The locale identifier
        public static string Locale => System.Globalization.CultureInfo.CurrentCulture.Name;
    }
}
