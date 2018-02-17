using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.ServiceBus.Messaging;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    public class FeedbackEventProcessor : IFeedbackEventProcessor, IDisposable
    {
        private readonly ILocationJerkLogic _locationJerkLogic;

        private EventProcessorHost _eventProcessorHost;
        private FeedbackProcessorFactory _factory;
        private IConfigurationProvider _configurationProvider;
        private CancellationTokenSource _cancellationTokenSource;
        private bool _isRunning = false;
        private bool _disposed = false;

        public FeedbackEventProcessor(
            ILifetimeScope lifetimeScope,
            ILocationJerkLogic locationJerkLogic)
        {
            _configurationProvider = lifetimeScope.Resolve<IConfigurationProvider>();
            _locationJerkLogic = locationJerkLogic;
        }

        public void Start()
        {
            _isRunning = true;
            _cancellationTokenSource = new CancellationTokenSource();
            Task.Run(() => this.StartProcessor(_cancellationTokenSource.Token), _cancellationTokenSource.Token);
        }

        public void Stop()
        {
            _cancellationTokenSource.Cancel();
            TimeSpan timeout = TimeSpan.FromSeconds(30);
            TimeSpan sleepInterval = TimeSpan.FromSeconds(1);
            while (_isRunning)
            {
                if (timeout < sleepInterval)
                {
                    break;
                }
                Thread.Sleep(sleepInterval);
            }
        }

        private async Task StartProcessor(CancellationToken token)
        {
            try
            {
                string hostName = Environment.MachineName;
                string eventHubPath = _configurationProvider.GetConfigurationSettingValue("FeedbackEventHub.Name").ToLowerInvariant();
                //string consumerGroup = EventHubConsumerGroup.DefaultGroupName;
                string consumerGroup = "feedbackcg";
                string eventHubConnectionString = _configurationProvider.GetConfigurationSettingValue("FeedbackEventHub.ConnectionString");
                string storageConnectionString = _configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");

                _eventProcessorHost = new EventProcessorHost(
                    hostName,
                    eventHubPath.ToLower(),
                    consumerGroup,
                    eventHubConnectionString,
                    storageConnectionString);

                _factory = new FeedbackProcessorFactory(
                    _locationJerkLogic,
                    _configurationProvider);

                Trace.TraceInformation("FeedbackEventProcessor: Registering host...");
                var options = new EventProcessorOptions();
                options.ExceptionReceived += OptionsOnExceptionReceived;
                await _eventProcessorHost.RegisterEventProcessorFactoryAsync(_factory);

                // processing loop
                while (!token.IsCancellationRequested)
                {
                    Trace.TraceInformation("FeedbackEventProcessor: Processing...");
                    await Task.Delay(TimeSpan.FromMinutes(5), token);
                }

                // cleanup
                await _eventProcessorHost.UnregisterEventProcessorAsync();
            }
            catch (Exception e)
            {
                Trace.TraceError("Error in FeedbackProcessor.StartProcessor, Exception: {0}", e.ToString());
            }
            _isRunning = false;
        }

        private void OptionsOnExceptionReceived(object sender, ExceptionReceivedEventArgs args)
        {
            Trace.TraceError("Received exception, action: {0}, exception: {1}", args.Action, args.Exception.ToString());
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Dispose();
                }
            }

            _disposed = true;
        }

        ~FeedbackEventProcessor()
        {
            Dispose(false);
        }
    }
}
