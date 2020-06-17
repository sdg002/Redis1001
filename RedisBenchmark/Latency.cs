namespace RedisBenchmark
{
    public class Latency
    {
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