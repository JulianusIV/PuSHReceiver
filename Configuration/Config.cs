namespace Configuration
{
    public class Config
    {
        public string CallbackUrl { get; set; }
        public string ConnectionString { get; set; }

        public Config(string callbackUrl, string connectionString)
        {
            CallbackUrl = callbackUrl;
            ConnectionString = connectionString;
        }
    }
}
