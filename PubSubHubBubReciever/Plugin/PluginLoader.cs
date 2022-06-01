using PubSubHubBubReciever.Plugin.Exceptions;
using PubSubHubBubReciever.Plugin.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace PubSubHubBubReciever.Plugin
{
    public class PluginLoader
    {
        private readonly List<IConsumerPlugin> _consumerPlugins = new();
        private readonly List<IPublisherPlugin> _publisherPlugins = new();

        public PluginLoader()
        {
            LoadDlls("Plugins");
            if (bool.Parse(Environment.GetEnvironmentVariable("LOADDEFAULTPLUGINS")))
                LoadDlls(".");

            _consumerPlugins = GetPlugins<IConsumerPlugin>();
            _publisherPlugins = GetPlugins<IPublisherPlugin>();
        }

        private List<T> GetPlugins<T>(List<T> plugins = null) where T : IBasePlugin
        {
            if (plugins is null)
                plugins = new();

            Type pluginType = typeof(T);

            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => pluginType.IsAssignableFrom(x) && x.IsClass)
                .ToArray();

            foreach (var type in types)
            {
                T plugin = (T)Activator.CreateInstance(type);
                if (plugins.Any(x => x.Name == plugin.Name))
                    throw new DuplicatePluginNameException("PluginLoader failed to load plugins, due to duplicate plugin names!", plugin.Name);
                plugins.Add(plugin);
                Console.WriteLine($"Loaded plugin {plugin.Name}.");
            }
            return plugins;
        }

        private void LoadDlls(string path)
        {
            if (Directory.Exists(path))
                Directory.GetFiles(path).ToList().ForEach(x => 
                {
                    if (x.EndsWith(".dll")) 
                        Assembly.LoadFile(Path.GetFullPath(x)); 
                });
        }

        public IConsumerPlugin ResolveParserPlugin(string name)
            => _consumerPlugins.SingleOrDefault(x => x.Name.Equals(name));

        public IPublisherPlugin ResolvePublishPlugin(string name)
            => _publisherPlugins.SingleOrDefault(x => x.Name.Equals(name));
    }
}
