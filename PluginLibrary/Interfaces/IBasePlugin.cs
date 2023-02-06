using PluginLibrary.PluginRepositories;
using Microsoft.Extensions.Logging;

namespace PluginLibrary.Interfaces
{
    public interface IBasePlugin
    {
        public string Name { get; }
        public IPluginRepository? PluginRepository { get; set; }
        public ILogger? Logger { get; set; }
        public Task InitAsync();
    }
}
