namespace Plugin.Exception
{
    internal class DuplicatePluginNameException : System.Exception
    {
        public string PluginName { get; private set; }

        public DuplicatePluginNameException(string name = null) : base()
        {
            PluginName = name;
        }

        public DuplicatePluginNameException(string message, string name = null) : base(message)
        {
            PluginName = name;
        }

        public DuplicatePluginNameException(string message, System.Exception innerException, string name = null) : base(message, innerException)
        {
            PluginName = name;
        }
    }
}
