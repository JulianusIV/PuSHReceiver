using Models;

namespace PluginLibrary.PluginRepositories
{
    public interface ILogRepository
    {
        public void CreateLogEntry(Log log);
    }
}
