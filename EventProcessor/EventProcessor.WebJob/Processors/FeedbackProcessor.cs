using System;
using System.Device.Location;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    public class FeedbackProcessor : IEventProcessor
    {
        private readonly ILocationJerkLogic _locationJerkLogic;
        private readonly ServiceClient _serviceClient;

        private int _totalMessages = 0;
        private Stopwatch _checkpointStopwatch;        

        public FeedbackProcessor(
            ILocationJerkLogic locationJerkLogic,
            IConfigurationProvider configurationProvider)
        {
            this.LastMessageOffset = "-1";
            var iotHubConnectionString = configurationProvider.GetConfigurationSettingValue("iotHub.ConnectionString");
            _locationJerkLogic = locationJerkLogic;
            _serviceClient = ServiceClient.CreateFromConnectionString(iotHubConnectionString);
        }

        public event EventHandler ProcessorClosed;

        public bool IsInitialized { get; private set; }

        public bool IsClosed { get; private set; }

        public bool IsReceivedMessageAfterClose { get; set; }

        public int TotalMessages
        {
            get { return _totalMessages; }
        }

        public CloseReason CloseReason { get; private set; }

        public PartitionContext Context { get; private set; }

        public string LastMessageOffset { get; private set; }

        public Task OpenAsync(PartitionContext context)
        {
            Trace.TraceInformation("FeedbackProcessor: Open : Partition : {0}", context.Lease.PartitionId);
            this.Context = context;
            _checkpointStopwatch = new Stopwatch();
            _checkpointStopwatch.Start();

            this.IsInitialized = true;

            return Task.Delay(0);
        }

        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            Trace.TraceInformation("FeedbackProcessor: In ProcessEventsAsync");
            IEnumerable<LocationJerkModel> locationJerks = await _locationJerkLogic.LoadLatestLocationJerkInfoAsync();

            foreach (EventData message in messages)
            {
                try
                {
                    Trace.TraceInformation("FeedbackProcessor: {0} - Partition {1}", message.Offset, context.Lease.PartitionId);
                    this.LastMessageOffset = message.Offset;

                    string jsonString = Encoding.UTF8.GetString(message.GetBytes());
                    IoTRawTelemetryModel rawData = null;
                    try
                    {
                        rawData = JsonConvert.DeserializeObject<IoTRawTelemetryModel>(jsonString);
                    }
                    catch (Exception ex)
                    {
                        continue;
                    }

                    if (rawData != null)
                    {
                        CurrentDeviceLocationModel item = new CurrentDeviceLocationModel()
                        {
                            DeviceId = rawData.DeviceId,
                            Latitude = rawData.Latitude,
                            Longitude = rawData.Longitude,
                            Altitude = rawData.Altitude,
                            Heading = rawData.Heading,
                            Speed = rawData.Speed,
                            MapIsBound = rawData.MapIsBound
                        };

                        if (item.MapIsBound)
                        {
                            var feedbackObject = new FeedbackModel(item);

                            var userLocation = new GeoCoordinate()
                            {
                                Latitude = item.Latitude,
                                Longitude = item.Longitude,
                                Altitude = item.Altitude,
                                Speed = item.Speed
                            };

                            feedbackObject.NearestJerks = GetNearestJerks(locationJerks, userLocation);

                            var feedbackString = JsonConvert.SerializeObject(feedbackObject);
                            Message msg = new Message(Encoding.ASCII.GetBytes(feedbackString));
                            try
                            {
                                await _serviceClient.SendAsync(item.DeviceId, msg);
                            }
                            catch (Exception ex)
                            {
                                if (!String.IsNullOrWhiteSpace(item.DeviceId))
                                {
                                    Trace.TraceError("FeedbackProcessor: Error in ProcessEventAsync -- Device: "+item.DeviceId+" -- " + ex.ToString());
                                }
                            }                            
                        }
                    }

                    ++_totalMessages;
                }
                catch (Exception e)
                {
                    Trace.TraceError("FeedbackProcessor: Error in ProcessEventAsync -- " + e.ToString());
                }
            }

            try
            {
                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - FeedbackProcessor.ProcessEventsAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);
            }

            if (this.IsClosed)
            {
                this.IsReceivedMessageAfterClose = true;
            }
        }

        private List<LocationModel> GetNearestJerks(IEnumerable<LocationJerkModel> blobLocations, GeoCoordinate userLocation)
        {
            List<LocationModel> nearestJerks = new List<LocationModel>();
            if (userLocation == null)
            {
                throw new ArgumentNullException("userLocation");
            }

            Func<double?, double?, double> getDistance = ProduceGetDistance(userLocation);

            if (blobLocations != null)
            {
                nearestJerks = blobLocations.Select(loc => new LocationModel {
                        Latitude = (double)loc.Latitude,
                        Longitude = (double)loc.Longitude,
                        Altitude = (double)loc.Altitude,
                        Status = loc.Status
                    }).Where(l => getDistance(l.Latitude,l.Longitude) < 0.1).ToList();
            }

            return nearestJerks;
        }

        private Func<double?, double?, double> ProduceGetDistance(GeoCoordinate userLocation)
        {
            if (userLocation == null)
            {
                throw new ArgumentNullException("userLocation");
            }

            return (lat, lng) => 
            {
                if (lat != null && lng != null)
                {
                    var location = new GeoCoordinate((double)lat, (double)lng);
                    return userLocation.GetDistanceTo(location) / 1000;
                }
                else
                {
                    return 100;
                }                
            };
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation("FeedbackProcessor: Close : Partition : " + context.Lease.PartitionId);
            this.IsClosed = true;
            _checkpointStopwatch.Stop();
            this.CloseReason = reason;
            this.OnProcessorClosed();

            try
            {
                return context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - FeedbackProcessor.CloseAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);

                return Task.Run(() => { });
            }
        }

        public virtual void OnProcessorClosed()
        {
            EventHandler handler = this.ProcessorClosed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
