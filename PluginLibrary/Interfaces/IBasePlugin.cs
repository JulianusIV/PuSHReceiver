using Microsoft.Extensions.Logging;
using PluginLibrary.PluginRepositories;

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
