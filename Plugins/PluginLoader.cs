﻿using Plugins.Exceptions;
using Plugins.Interfaces;
using System.Reflection;

namespace Plugins
{
    public class PluginLoader
    {
        private readonly List<IConsumerPlugin> _consumerPlugins = new();
        private readonly List<IPublisherPlugin> _publisherPlugins = new();

        public PluginLoader()
        {
            LoadDlls("Plugins");
            var envVar = Environment.GetEnvironmentVariable("LOADDEFAULTPLUGINS");
            bool loadDefault = envVar is null || bool.Parse(envVar);
            if (loadDefault)
                LoadDlls(".");

            _consumerPlugins = GetPlugins<IConsumerPlugin>();
            _publisherPlugins = GetPlugins<IPublisherPlugin>();
        }

        private List<T> GetPlugins<T>(List<T>? plugins = null) where T : IBasePlugin
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
                T? plugin = (T?)Activator.CreateInstance(type);
                if (plugin is null)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to create instance of plugin {type.Name}!");
                    Console.ForegroundColor = ConsoleColor.Gray;
                    continue;
                }
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

        public T ResolvePlugin<T>(string name) where T : IBasePlugin
        {
            T? retVal;
            if (typeof(T) == typeof(IConsumerPlugin))
                retVal = (T?)ResolveConsumerPlugin(name);
            else if (typeof(T) == typeof(IPublisherPlugin))
                retVal = (T?)ResolvePublishPlugin(name);
            else 
                retVal = default;
            if (retVal is null)
                throw new PluginNotFoundException(name);
            return retVal;
        }

        private IConsumerPlugin? ResolveConsumerPlugin(string name) 
            => _consumerPlugins.SingleOrDefault(x => x.Name.Equals(name));

        private IPublisherPlugin? ResolvePublishPlugin(string name) 
            => _publisherPlugins.SingleOrDefault(x => x.Name.Equals(name));
    }
}
