namespace RedisBenchmark
{
    /// <summary>
    /// All timing values are in milliseconds
    /// </summary>
    public class Latency
    {
        /// <summary>
        /// Actual count of operations performed
        /// </summary>
        public int Count { get; set; }

        /// <summary>
        /// Size of the document used for the cache operations (bytes)
        /// </summary>
        public int AveragePayloadSize { get; set; }

        public double MaxLatency { get; set; }
        public double MinLatency { get; set; }
        public double MeanLatency { get; internal set; }
        public double NinetyFiveReadPercentLatency { get; internal set; }

        public override string ToString()
        {
            return $"MaxLatency={MaxLatency}    MinLatency={MinLatency} MeanLatency={MeanLatency}   95%={NinetyFiveReadPercentLatency}";
        }
    }
}