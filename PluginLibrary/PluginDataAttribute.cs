using PluginLibrary.Interfaces;

namespace PluginLibrary
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PluginDataAttribute : Attribute
    {
        public Type Plugin { get; init; }

        public PluginDataAttribute(Type plugin)
        {
            if (!typeof(IBasePlugin).IsAssignableFrom(plugin))
                throw new ArgumentException($"Type has to be subtype of {nameof(IBasePlugin)}!");
            Plugin = plugin;
        }
    }
}
