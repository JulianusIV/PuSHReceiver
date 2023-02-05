namespace PluginLoader.Exceptions
{
    internal class PluginNotFoundException : Exception
    {
        public string Name { get; set; }
        public PluginNotFoundException(string name) : base()
            => Name = name;

        public PluginNotFoundException(string message, string name) : base(message)
            => Name = name;

        public PluginNotFoundException(string message, Exception innerException, string name) : base(message, innerException)
            => Name = name;
    }
}
