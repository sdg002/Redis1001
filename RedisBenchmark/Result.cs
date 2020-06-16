using System;
using System.Collections.Generic;
using System.Text;

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
        public double MeanLatency { get; internal set; }
        public double NinetyFivePercentLatency { get; internal set; }
    }
}