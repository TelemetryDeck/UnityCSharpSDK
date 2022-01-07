using System;

namespace TelemetryClient
{
    /// Configuration for TelemetryManager
    ///
    /// Use an instance of this class to specify settings for TelemetryManager. If these settings change during the course of
    /// your runtime, it might be a good idea to hold on to the instance and update it as needed. TelemetryManager's behaviour
    /// will update as well.
    public sealed class TelemetryManagerConfiguration
    {
        private const string TELEMETRY_API_BASE_URL = "https://nom.telemetrydeck.com";

        /// Your app's ID for Telemetry. Set this during initialization.
        public string TelemetryAppID { get; private set; }

        /// The domain to send signals to. Defaults to the default Telemetry API server.
        /// (Don't change this unless you know exactly what you're doing)
        public string ApiBaseUrl { get; private set; }

        /// Instead of specifying a user identifier with each `send` call, you can set your user's name/email/identifier here and
        /// it will be sent with every signal from now on.
        ///
        /// Note that just as with specifying the user identifier with the `send` call, the identifier will never leave the device.
        /// Instead it is used to create a hash, which is included in your signal to allow you to count distinct users.
        public string defaultUser;

        /// If `true`, sends a "newSessionBegan" Signal on each app foreground or cold launch
        ///
        /// Defaults to true. Set to false to prevent automatically sending this signal.
        public bool sendNewSessionBeganSignal = true;

        private Guid _sessionId;

        /// A random identifier for the current user session.
        ///
        /// On iOS, tvOS, and watchOS, the session identifier will automatically update whenever your app returns from background, or if it is
        /// launched from cold storage. On other platforms, a new identifier will be generated each time your app launches. If you'd like
        /// more fine-grained session support, write a new random session identifier into this property each time a new session begins.
        ///
        /// Beginning a new session automatically sends a "newSessionBegan" Signal if `sendNewSessionBeganSignal` is `true`
        public Guid SessionId
        {
            get => _sessionId;
            set
            {
                _sessionId = value;
                if (sendNewSessionBeganSignal)
                {
                    TelemetryManager.SendSignal("newSessionBegan");
                }
            }
        }

        /// <summary>
        /// Superseded by <see cref="IsTestMode"/>.
        /// If <c>true</c>, sends signals even if your scheme's build configuration is set to Debug.
        ///
        /// Defaults to false, which only sends signals if not running in Unity Editor, 
        /// and if your build configuration is not set to Debug build.
        /// </summary>
        [Obsolete("Please use the IsTestMode property instead.", error: true)]
        public bool sendSignalsInEditorAndDebug = false;

        /// <summary>
        /// If <c>true</c>, marks signals sent to the server as "test mode".
        /// <br/>
        /// Defaults to <c>true</c> in the Unity Editor and
        /// if your build configuration is set to Debug build.
        /// Defaults to <c>false</c> otherwise.
        /// <br/>
        /// You may set this property to override the default behaviour.
        /// </summary>
        public bool IsTestMode
        {
            get
            {
                if (_testModeOverride.HasValue)
                {
                    return (bool)_testModeOverride;
                }
                else
                {
#if UNITY_EDITOR || DEBUG
                    return true;
#else
                    return false;
#endif
                }
            }
            set
            {
                _testModeOverride = value;
            }
        }
        private bool? _testModeOverride = null;

        /// Log the current status to the signal cache to the console.
        public bool showDebugLogs = false;

        /// <summary>
        /// Creates a new TelemetryManagerConfiguration.
        /// </summary>
        /// <param name="appId"></param>
        /// <param name="baseUrl">The domain to send signals to. Defaults to the default Telemetry API server.</param>
        public TelemetryManagerConfiguration(string appId, string baseUrl = null)
        {
            TelemetryAppID = appId;

            if (!string.IsNullOrEmpty(baseUrl))
            {
                ApiBaseUrl = baseUrl;
            }
            else
            {
                ApiBaseUrl = TELEMETRY_API_BASE_URL;
            }

            _sessionId = Guid.NewGuid();
        }
    }
}
