namespace Contracts.Service
{
    public interface IShutdownService
    {
        public void Shutdown();
        public void TriggerAllSubsUnsubscribed();
    }
}
