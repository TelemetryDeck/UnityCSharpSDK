using System;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

namespace TelemetryClient
{
    /// <summary>
    /// A local cache for signals to be sent to the TelemetryDeck ingestion service
    ///
    /// There is no guarantee that Signals come out in the same order you put them in. This shouldn't matter though,
    /// since all Signals automatically get a `receivedAt` property with a date, allowing the server to reorder them
    /// correctly.
    ///
    /// The cache is backed up to disk so Signals are not lost if the app restarts.
    /// </summary>
    /// <typeparam name="T">A [Serializable] type.</typeparam>
    internal class SignalCache<T> where T : new()
    {
        private const int MaxNumberOfSignalsToSendInBatch = 100;

        public bool showDebugLogs = false;

        private readonly Queue<T> cachedSignals;

        /// <summary>
        /// How many Signals are cached
        /// </summary>
        public int Count
        {
            get
            {
                lock (this)
                {
                    return cachedSignals.Count;
                }
            }
        }

        /// <summary>
        /// Insert a Signal into the cache
        /// </summary>
        public void Push(T signal)
        {
            lock (this)
            {
                cachedSignals.Enqueue(signal);
            }
        }

        /// <summary>
        /// Insert a number of Signals into the cache
        /// </summary>
        public void Push(IList<T> signals)
        {
            lock (this)
            {
                foreach (var signal in signals)
                    cachedSignals.Enqueue(signal);
            }
        }

        /// <summary>
        /// Remove a number of Signals from the cache and return them
        ///
        /// You should hold on to the signals returned by this function. If the action you are trying to do with them fails
        /// (e.g. sending them to a server) you should reinsert them into the cache with the `push` function.
        /// </summary>
        public List<T> Pop()
        {
            List<T> poppedSignals = new List<T>();

            lock (this)
            {
                int sliceSize = Math.Min(MaxNumberOfSignalsToSendInBatch, cachedSignals.Count);
                for (int i = 0; i < sliceSize; i++)
                {
                    poppedSignals.Add(cachedSignals.Dequeue());
                }
            }

            return poppedSignals;
        }

        private string FileUrl
        {
            get
            {
                return $"{Application.persistentDataPath}/telemetrysignalcache";
            }
        }

        /// <summary>
        /// Saves the entire signal cache to disk and clears the SignalCache.
        /// </summary>
        /// <exception cref="IOException">If the cache file could not be written to.</exception>
        public void BackupCache()
        {
            lock (this)
            {
                try
                {
                    var data = JsonConvert.SerializeObject(cachedSignals);
                    File.WriteAllText(FileUrl, data);

                    if (showDebugLogs)
                        Debug.Log($"Saved Telemetry cache {data} of {cachedSignals.Count} signals");

                    /// After saving the cache, we need to clear our local cache otherwise
                    /// it could get merged with the cache read back from disk later if
                    /// it's still in memory
                    cachedSignals.Clear();
                }
                catch (IOException e)
                {
                    Debug.LogError("Error while saving Telemetry cache");
                    throw e;
                }
            }
        }

        /// <summary>
        /// Loads any previous signal cache from disk
        /// </summary>
        public SignalCache(bool showDebugLogs)
        {
            this.showDebugLogs = showDebugLogs;
            string cacheFilePath = FileUrl;

            if (showDebugLogs)
                Debug.Log($"Loading Telemetry cache from: {cacheFilePath}");

            try
            {
                var data = File.ReadAllText(cacheFilePath);
                /// Loaded cache file, now delete it to stop it being loaded multiple times
                File.Delete(cacheFilePath);

                /// Decode the data into a new cache
                List<T> signals = JsonConvert.DeserializeObject<List<T>>(data);
                cachedSignals = new Queue<T>(signals);

                if (showDebugLogs)
                    Debug.Log($"Loaded {signals.Count} signals");
            }
            catch
            {
                /// failed to load cache file; that's okay - maybe it has been loaded already
                /// or it hasn't been saved yet
                cachedSignals = new Queue<T>();
            }
        }
    }
}
