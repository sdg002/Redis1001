namespace FunctionApp1
{
    public class RedisConfiguration
    {
        public string ConnectionStringAdmin => $"{this.ConnectionStringTxn},allowAdmin=true";

        public string ConnectionStringTxn { get; internal set; }

        public override string ToString()
        {
            return $"{ConnectionStringTxn}";
        }
    }
}