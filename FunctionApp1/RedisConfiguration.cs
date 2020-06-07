namespace FunctionApp1
{
    public class RedisConfiguration
    {
        public string ConnectionStringAdmin { get; internal set; }

        public string ConnectionStringTxn { get; internal set; }

        public override string ToString()
        {
            return $"{ConnectionStringTxn}";
        }
    }
}