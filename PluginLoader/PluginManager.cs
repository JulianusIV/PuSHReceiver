using Contracts;
using Microsoft.Extensions.Logging;
using PluginLibrary;
using PluginLibrary.Interfaces;
using PluginLibrary.PluginRepositories;
using PluginLoader.Exceptions;
using System.Reflection;

namespace PluginLoader
{
    public class PluginManager : IPluginManager
    {
        private Dictionary<IConsumerPlugin, Type?> _consumerPlugins;
        private Dictionary<IPublisherPlugin, Type?> _publisherPlugins;
        private readonly ILogger<PluginManager> _logger;
        private readonly IPluginRepository _pluginRepository;

        //disable nullable warning - non nullable properties are set in ReloadPlugins method
#pragma warning disable CS8618
        public PluginManager(ILogger<PluginManager> logger, IPluginRepository pluginRepository)
        {
            _logger = logger;
            _pluginRepository = pluginRepository;
            //populate plugin dictionaries
            ReloadPlugins();
        }
#pragma warning restore CS8618

        public void ReloadPlugins()
        {
            //load user defined plugins (load from bin folder when debugging
#if DEBUG
            LoadDlls(@"bin\Debug\net7.0\Plugins");
#else
            LoadDlls(@"Plugins");
#endif
            //if envvar is set to true or null load default plugins from working directory (bin folder when debugging)
            var envVar = Environment.GetEnvironmentVariable("LOADDEFAULTPLUGINS");
            bool loadDefault = envVar is null || bool.Parse(envVar);
            if (loadDefault)
            {
#if DEBUG
                LoadDlls(@"bin\Debug\net7.0");
#else
                LoadDlls(@".");
#endif
            }
            //fetch plugins from loaded dlls
            _consumerPlugins = GetPlugins<IConsumerPlugin>();
            _publisherPlugins = GetPlugins<IPublisherPlugin>();
        }

        /// <summary>
        /// Fetches a loaded plugin from cache
        /// </summary>
        /// <typeparam name="T">Plugin interface type - Consumer or Publisher</typeparam>
        /// <param name="name">Plugin name</param>
        /// <returns>An instance of the plugin</returns>
        /// <exception cref="PluginNotFoundException">Thrown when requested plugin name could not be found</exception>
        public T ResolvePlugin<T>(string name) where T : IBasePlugin
        {
            T? plugin = default;

            if (typeof(T) == typeof(IConsumerPlugin))
                plugin = (T?)ResolveConsumerPlugin(name);
            else if (typeof(T) == typeof(IPublisherPlugin))
                plugin = (T?)ResolvePublishPlugin(name);

            if (plugin is null)
            {
                _logger.LogError("Could not resolve plugin name {Name}!", name);
                throw new PluginNotFoundException(name);
            }

            return plugin;
        }

        /// <summary>
        /// Fetches the datatype of a loaded plugin from cache
        /// </summary>
        /// <typeparam name="T">Plugin interface type - Consumer or Publisher</typeparam>
        /// <param name="name">Plugin name</param>
        /// <returns>The corresponding data type or null if not found or no data type exists for the plugin</returns>
        public Type? ResolvePluginDataType<T>(string name) where T : IBasePlugin
        {
            Type? type = null;
            if (typeof(T) == typeof(IConsumerPlugin))
                type = ResolveConsumerData(name);
            else if (typeof(T) == typeof(IPublisherPlugin))
                type = ResolvePublishData(name);

            return type;
        }

        private IConsumerPlugin? ResolveConsumerPlugin(string name)
            => _consumerPlugins.SingleOrDefault(x => x.Key.Name.Equals(name)).Key;

        private IPublisherPlugin? ResolvePublishPlugin(string name)
            => _publisherPlugins.SingleOrDefault(x => x.Key.Name.Equals(name)).Key;

        private Type? ResolveConsumerData(string name)
            => _consumerPlugins.SingleOrDefault(x => x.Key.Name.Equals(name)).Value;

        private Type? ResolvePublishData(string name)
            => _publisherPlugins.SingleOrDefault(x => x.Key.Name.Equals(name)).Value;

        private Dictionary<T, Type?> GetPlugins<T>() where T : IBasePlugin
        {
            Dictionary<T, Type?> plugins = new();
            Type interfaceType = typeof(T);


            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var types = assemblies.SelectMany(x => x.GetTypes());


            //get all possible plugin types
            var pluginImplementations = types.Where(x => interfaceType.IsAssignableFrom(x) && x.IsClass);

            //get all possible plugin data types
            var dataTypes = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => x.GetCustomAttribute<PluginDataAttribute>() is not null && x.IsClass);

            foreach (var type in pluginImplementations)
            {
                //create an instance of the type
                T? plugin = (T?)Activator.CreateInstance(type);
                if (plugin is null)
                {
                    _logger.LogWarning("Failed to create instance of plugin {Name}!", type.Name);
                    continue;
                }
                //check for duplicates
                if (plugins.Any(x => x.Key.Name == plugin.Name))
                {
                    _logger.LogError("Pluginloader failed to load plugins due to duplicate plugin name {Name}!", plugin.Name);
                    throw new DuplicatePluginNameException("Pluginloader failed to load plugins due to duplicate plugin name!", plugin.Name);
                }
                //DI repo into plugin
                plugin.PluginRepository = _pluginRepository;
                //asyncronously call init
                _ = plugin.InitAsync();
                //find plugin data type and add to dictionary
                plugins.Add(plugin, dataTypes.FirstOrDefault(x => x.GetCustomAttribute<PluginDataAttribute>()!.Plugin == type));
                _logger.LogInformation("Loaded plugin {Name}.", plugin.Name);
            }
            return plugins;
        }

        /// <summary>
        /// Loads all .dll files located inside give directory
        /// </summary>
        /// <param name="path">the directory to load from</param>
        private void LoadDlls(string path)
        {
            if (Directory.Exists(path))
            {
                Directory.GetFiles(path).ToList().ForEach(x =>
                {
                    if (x.EndsWith(".dll"))
                    {
                        try
                        {
                            Assembly.LoadFile(Path.GetFullPath(x));
                        }
                        catch (Exception)
                        {
                            _logger.LogWarning("Failed to load dll {dllname}.", x);
                        }
                    }
                });
            }
        }
    }
}