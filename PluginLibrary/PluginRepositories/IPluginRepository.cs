using Models;

namespace PluginLibrary.PluginRepositories
{
    public interface IPluginRepository
    {
        public void SaveData(Lease toSave);
    }
}
