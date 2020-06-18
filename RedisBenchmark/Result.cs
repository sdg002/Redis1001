namespace RedisBenchmark
{
    /// <summary>
    /// Benchmark results
    /// </summary>
    public class Result
    {
        public int Iterations { get; set; }
        public int TotalReads { get; internal set; }
        public int TotalWrites { get; internal set; }
        public Latency ReadLatency { get; set; }
        public Latency WriteLatency { get; set; }
        public int Allocations { get; internal set; }

        /// <summary>
        /// Total time take for the test (ms)
        /// </summary>
        public long TotalTime { get; internal set; }
    }
}