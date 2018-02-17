namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    public interface IFeedbackEventProcessor
    {
        void Start();
        void Stop();
    }
}
