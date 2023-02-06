namespace Configuration
{
    public static class ConfigurationManager
    {
        public static WebConfig WebConfig
        {
            get
            {
                _webConfig ??= new(string.Empty);
                return _webConfig;
            }
            set => _webConfig = value;
        }

        public static DbConfig DbConfig
        {
            get
            {
                _dbConfig ??= new(string.Empty);
                return _dbConfig;
            }
            set => _dbConfig = value;
        }

        public static PluginsConfig PluginsConfig
        {
            get
            {
                _pluginsConfig ??= new();
                return _pluginsConfig;
            }
            set => _pluginsConfig = value;
        }

        private static WebConfig? _webConfig;
        private static DbConfig? _dbConfig;
        private static PluginsConfig? _pluginsConfig;
    }
}