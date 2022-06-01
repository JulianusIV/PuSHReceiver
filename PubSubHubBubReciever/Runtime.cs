using PubSubHubBubReciever.Plugin;
using Services.Service;

namespace PubSubHubBubReciever
{
    public class Runtime
    {
        #region Singleton
        private static Runtime _instance;
        private static readonly object _lock = new();
        public static Runtime Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance is null)
                        _instance = new();
                    return _instance;
                }
            }
        }
        #endregion

        #region Construction/Destruction
        private Runtime()
        {
            _pluginLoader = new();
            _serviceLoader = new();
        }
        #endregion

        #region Plugin
        public PluginLoader PluginLoader
        {
            get
            {
                if (_pluginLoader is null)
                    _pluginLoader = new();
                return _pluginLoader;
            }
        }

        private PluginLoader _pluginLoader;
        #endregion

        #region Service
        public ServiceLoader ServiceLoader
        {
            get
            {
                if (_serviceLoader is null)
                    _serviceLoader = new();
                return _serviceLoader;
            }
        }

        private ServiceLoader _serviceLoader;
        #endregion
    }
}
