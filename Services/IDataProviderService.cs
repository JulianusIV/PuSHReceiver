using System.Runtime.CompilerServices;

namespace Services
{
    public interface IDataProviderService
    {
        public Data.JSONObjects.Data? Data { get; set; }

        public void Load();

        public void Save();
    }
}
