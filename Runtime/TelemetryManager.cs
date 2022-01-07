using System;
using System.Collections.Generic;

namespace TelemetryClient
{
    using AdditionalPayload = Dictionary<string, string>;
    using TelemetrySignalType = String;

    /// Accepts signals that signify events in your app's life cycle, collects and caches them, and pushes them to the Telemetry API.
    ///
    /// Use an instance of `TelemetryManagerConfiguration` to configure this at initialization and during its lifetime.
    public class TelemetryManager
    {
        internal const string TelemetryClientVersion = "UnityCSharpClient 1.1.6";

        private static InvalidOperationException NotInitializedException
        {
            get
            {
                return new InvalidOperationException(
                    "Please call TelemetryManager.Initialize(...) before accessing the shared telemetryManager instance.");
            }
        }

        private static TelemetryManager _instance;

        private TelemetryManagerConfiguration configuration;

        private SignalManager signalManager;

        private TelemetryManager(TelemetryManagerConfiguration configuration)
        {
            this.configuration = configuration;
            signalManager = SignalManager.CreateSignalManager(configuration: configuration);
        }

        #region API
        public static void Initialize(TelemetryManagerConfiguration configuration)
        {
            _instance = new TelemetryManager(configuration: configuration);
        }

        public static TelemetryManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    throw NotInitializedException;
                }
                return _instance;
            }
        }

        /// <summary>
        /// Returns <c>true</c> if the TelemetryManager has been 
        /// <see cref="Initialize(TelemetryManagerConfiguration)">initialized</see> correctly,
        /// <c>false</c> otherwise. <br/>
        /// </summary>
        public static bool IsInitialized => _instance != null;

        /// <summary>
        /// Shuts down the SDK and deinitializes the current <c>TelemetryManager</c>.
        /// 
        /// Once called, you must call <see cref="Initialize(TelemetryManagerConfiguration)"/> again before sending additional signals.
        /// </summary>
        /// <exception cref="InvalidOperationException">If the SDK was not initialized.</exception>
        public static void Terminate()
        {
            if (_instance == null)
            {
                throw NotInitializedException;
            }
            _instance.signalManager.Terminate();
            _instance = null;
        }

        /// Change the default user identifier sent with each signal.
        ///
        /// Instead of specifying a user identifier with each `send` call, you can set your user's name/email/identifier here and
        /// it will be sent with every signal from now on. If you still specify a user in the `send` call, that takes precedence.
        ///
        /// Set to `nil` to disable this behavior.
        ///
        /// Note that just as with specifying the user identifier with the `send` call, the identifier will never leave the device.
        /// Instead it is used to create a hash, which is included in your signal to allow you to count distinct users.
        public void UpdateDefaultUser(string newDefaultUser = null)
        {
            configuration.defaultUser = newDefaultUser;
        }

        /// Generate a new Session ID for all new Signals, in order to begin a new session instead of continuing the old one.
        public void GenerateNewSession()
        {
            configuration.SessionId = Guid.NewGuid();
        }

        /// <summary>
        /// Sends a Signal to TelemetryDeck, to record that an event has occurred. <br/>
        /// If you specify a user identifier here, it will take precedence over
        /// the default user identifier specified in the <see cref="TelemetryManagerConfiguration"/>.
        /// If you specify a payload, it will be sent in addition to the default payload which includes OS Version, App Version, and more.
        /// </summary>
        /// <param name="signalType">Name of the event that occurred</param>
        /// <param name="clientUser">Optional: user identifier to send instead of the one from the <c>TelemetryManagerConfiguration</c></param>
        /// <param name="additionalPayload">Optional: additional key-value pairs to be sent with the signal</param>
        public static void SendSignal(TelemetrySignalType signalType, string clientUser = null, AdditionalPayload additionalPayload = null)
        {
            Instance.Send(signalType, clientUser, additionalPayload);
        }

        /// <summary>
        /// Sends a Signal to TelemetryDeck, to record that an event has occurred. <br/>
        /// If you specify a user identifier here, it will take precedence over
        /// the default user identifier specified in the <see cref="TelemetryManagerConfiguration"/>.
        /// If you specify a payload, it will be sent in addition to the default payload which includes OS Version, App Version, and more.
        /// </summary>
        /// <param name="signalType">Name of the event that occurred</param>
        /// <param name="clientUser">Optional: user identifier to send instead of the one from the <c>TelemetryManagerConfiguration</c></param>
        /// <param name="additionalPayload">Optional: additional key-value pairs to be sent with the signal</param>
        public void Send(TelemetrySignalType signalType, string clientUser = null, AdditionalPayload additionalPayload = null)
        {
            signalManager.ProcessSignal(configuration, signalType, clientUser, additionalPayload);
        }
    }
    #endregion
}
