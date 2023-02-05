using PluginLibrary.Interfaces;

namespace Contracts
{
    public interface IPluginManager
    {
        public T ResolvePlugin<T>(string name) where T : IBasePlugin;
        public Type? ResolvePluginDataType<T>(string name) where T : IBasePlugin;
    }
}
