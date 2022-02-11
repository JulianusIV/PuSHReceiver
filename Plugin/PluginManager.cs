using Plugin.Exception;
using Plugin.ParserPlugin;
using Plugin.PublisherPlugin;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Plugin
{
    public class PluginManager
    {
        #region Singleton
        private static readonly object _instanceLock = new object();
        private static PluginManager _instance;
        public static PluginManager Instance
        {
            get
            {
                lock (_instanceLock)
                {
                    if (_instance is null)
                        _instance = new PluginManager();
                    return _instance;
                }
            }
        }
        private PluginManager() { }
        #endregion

        private readonly List<IParserPlugin> parserPlugins = new List<IParserPlugin>();
        private readonly List<IPublishPlugin> publishPlugins = new List<IPublishPlugin>();

        public void Load()
        {
            if (Directory.Exists("Plugins"))
            {
                var files = Directory.GetFiles("Plugins");
                foreach (var file in files)
                    if (file.EndsWith(".dll"))
                        Assembly.LoadFile(Path.GetFullPath(file));
            }

            Type parserPluginType = typeof(IParserPlugin);

            Type[] types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => parserPluginType.IsAssignableFrom(x) && x.IsClass)
                .ToArray();

            foreach (var type in types)
            {
                IParserPlugin plugin = (IParserPlugin)Activator.CreateInstance(type);
                if (parserPlugins.Any(x => x.Name == plugin.Name))
                    throw new DuplicatePluginNameException("Plugin manager failed to load parser plugins, due to duplicate plugin names!", plugin.Name);
                parserPlugins.Add(plugin);
                Console.WriteLine($"Loaded plugin {plugin.Name}.");
            }

            Type publishPluginType = typeof(IPublishPlugin);

            types = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.GetTypes())
                .Where(x => publishPluginType.IsAssignableFrom(x) && x.IsClass)
                .ToArray();

            foreach (var type in types)
            {
                IPublishPlugin plugin = (IPublishPlugin)Activator.CreateInstance(type);
                if (publishPlugins.Any(x => x.Name == plugin.Name))
                    throw new DuplicatePluginNameException("Plugin manager failed to load publish plugins, due to duplicate plugin names!", plugin.Name);
                publishPlugins.Add(plugin);
                Console.WriteLine($"Loaded plugin {plugin.Name}.");
            }
        }

        public IParserPlugin ResolveParserPlugin(string name)
            => parserPlugins.SingleOrDefault(x => x.Name.Equals(name));

        public IPublishPlugin ResolvePublishPlugin(string name)
            => publishPlugins.SingleOrDefault(x => x.Name.Equals(name));
    }
}
