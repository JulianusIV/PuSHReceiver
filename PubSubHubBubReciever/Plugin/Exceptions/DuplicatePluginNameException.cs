using System;

namespace PubSubHubBubReciever.Plugin.Exceptions
{
    internal class DuplicatePluginNameException : Exception
    {
        public string PluginName { get; private set; }

        public DuplicatePluginNameException(string name = null) : base()
            => PluginName = name;

        public DuplicatePluginNameException(string message, string name = null) : base(message)
            => PluginName = name;

        public DuplicatePluginNameException(string message, Exception innerException, string name = null) : base(message, innerException)
            => PluginName = name;
    }
}
