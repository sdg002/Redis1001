namespace FunctionApp1
{
    public class RedisConfig
    {
        public string Server { get; set; }
        public string Port { get; set; }

        public override string ToString()
        {
            return $"Server={Server}    Port={Port}";
        }
    }
}