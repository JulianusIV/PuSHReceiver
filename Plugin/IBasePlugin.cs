using DataLayer.JSONObject;

namespace Plugin
{
    public interface IBasePlugin
    {
        public string Name { get; }
        public string TopicAdded(DataSub dataSub, params string[] additionalInfo);
        public string TopicUpdated(DataSub dataSub, DataSub oldData, params string[] additionalInfo);
    }
}
