using Services.Interfaces;

namespace Services
{
    public interface IDataProviderService : IService
    {
        public Data.JSONObjects.Data Data { get; set; }

        public void Save();
    }
}
