namespace Configuration
{
    public class WebConfig
    {
        public string CallbackUrl { get; set; }

        public WebConfig(string callbackUrl)
        {
            CallbackUrl = callbackUrl;
        }
    }

    public class DbConfig
    {
        public string ConnectionString { get; set; }

        public DbConfig(string connectionString)
        {
            ConnectionString = connectionString;
        }
    }

    public class PluginsConfig
    {
        public bool UseDefaultPlugins { get; set; } = true;
    }
}
