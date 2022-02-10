using DataLayer.JSONObject;

namespace Plugin
{
    public interface IBasePlugin
    {
        public string Name { get; protected set; }
        public string TopicAdded(DataSub dataSub, params string[] additionalInfo);
        public string TopicUpdated(DataSub dataSub, DataSub oldData, params string[] additionalInfo);
    }
}
