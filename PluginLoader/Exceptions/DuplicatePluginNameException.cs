namespace PluginLoader.Exceptions
{
    internal class DuplicatePluginNameException : Exception
    {
        public string PluginName { get; private set; }

        public DuplicatePluginNameException(string name) : base()
            => PluginName = name;

        public DuplicatePluginNameException(string message, string name) : base(message)
            => PluginName = name;

        public DuplicatePluginNameException(string message, Exception innerException, string name) : base(message, innerException)
            => PluginName = name;
    }
}
