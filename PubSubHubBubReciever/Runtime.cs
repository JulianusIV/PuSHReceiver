using Plugins;
using Services;
using System.Threading;

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
                    _instance ??= new();
                    return _instance;
                }
            }
        }
        #endregion

        #region Construction/Destruction
        private Runtime()
        {
            var dataProvider = ServiceLoader.ResolveService<IDataProviderService>();
            var lease = ServiceLoader.ResolveService<ILeaseService>();
            var url = dataProvider.Data.CallbackURL;
            _serviceLoader = new();
            _pluginLoader = new(url, dataProvider, lease);
        }
        #endregion

        #region Properties
        public CancellationTokenSource TokenSource = new(); 
        #endregion

        #region Plugin
        public PluginLoader PluginLoader
        {
            get
            {
                if (_pluginLoader is null)
                {
                    var dataProvider = ServiceLoader.ResolveService<IDataProviderService>();
                    var lease = ServiceLoader.ResolveService<ILeaseService>();
                    var url = dataProvider.Data.CallbackURL;
                    _pluginLoader = new(url, dataProvider, lease);
                }
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
                _serviceLoader ??= new();
                return _serviceLoader;
            }
        }

        private ServiceLoader _serviceLoader;
        #endregion
    }
}
