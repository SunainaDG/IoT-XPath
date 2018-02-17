using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Globalization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.ServiceBus.Messaging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.EventProcessor.WebJob.Processors
{
    public class ActionProcessor : IEventProcessor
    {
        private readonly IActionLogic _actionLogic;
        private readonly ILocationJerkLogic _locationJerkLogic;
        private readonly IActionMappingLogic _actionMappingLogic;
        private readonly IConfigurationProvider _configurationProvider;

        private int _totalMessages = 0;
        private Stopwatch _checkpointStopwatch;

        public ActionProcessor(
            IActionLogic actionLogic,
            ILocationJerkLogic locationJerkLogic,
            IActionMappingLogic actionMappingLogic,
            IConfigurationProvider configurationProvider)
        {
            this.LastMessageOffset = "-1";
            _actionLogic = actionLogic;
            _actionMappingLogic = actionMappingLogic;
            _configurationProvider = configurationProvider;
            _locationJerkLogic = locationJerkLogic;
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
            Trace.TraceInformation("ActionProcessor: Open : Partition : {0}", context.Lease.PartitionId);
            this.Context = context;
            _checkpointStopwatch = new Stopwatch();
            _checkpointStopwatch.Start();

            this.IsInitialized = true;

            return Task.Delay(0);
        }


        public async Task ProcessEventsAsync(PartitionContext context, IEnumerable<EventData> messages)
        {
            Trace.TraceInformation("ActionProcessor: In ProcessEventsAsync");
            List<LocationJerkModel> newLocationJerks = new List<LocationJerkModel>();

            foreach (EventData message in messages)
            {
                try
                {
                    Trace.TraceInformation("ActionProcessor: {0} - Partition {1}", message.Offset, context.Lease.PartitionId);
                    this.LastMessageOffset = message.Offset;

                    string jsonString = Encoding.UTF8.GetString(message.GetBytes());
                    List<TelemetryJerkModel> convertedList = JsonConvert.DeserializeObject<List<TelemetryJerkModel>>(jsonString);
                    var results = convertedList.Distinct(new TelemetryJerkComparer());
                    if (results != null)
                    {
                        foreach (TelemetryJerkModel item in results)
                        {
                           UpdateLocationJerkList(GetLocationJerkFromTelemetry(item),ref newLocationJerks);
                        }
                    }

                    ++_totalMessages;
                }
                catch (Exception e)
                {
                    Trace.TraceError("ActionProcessor: Error in ProcessEventAsync -- " + e.ToString());
                }
            }
            //finally save data to blob
            try
            {
                await SaveToLocationJerkInfo(newLocationJerks);
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** SaveLocationJerkInfo Exception - ActionProcessor.ProcessEventsAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);
            }

            // checkpoint after processing batch
            try
            {
                await context.CheckpointAsync();
            }
            catch (Exception ex)
            {
                Trace.TraceError(
                    "{0}{0}*** CheckpointAsync Exception - ActionProcessor.ProcessEventsAsync ***{0}{0}{1}{0}{0}",
                    Console.Out.NewLine,
                    ex);
            }

            if (this.IsClosed)
            {
                this.IsReceivedMessageAfterClose = true;
            }
        }

        private LocationJerkModel GetLocationJerkFromTelemetry(TelemetryJerkModel eventData)
        {
            LocationJerkModel newLocationJerkModel = null;

            if (eventData == null)
            {
                Trace.TraceWarning("Action event is null");
                return newLocationJerkModel;
            }

            try
            {
                // NOTE: all column names from ASA come out as lowercase; see 
                // https://social.msdn.microsoft.com/Forums/office/en-US/c79a662b-5db1-4775-ba1a-23df1310091d/azure-table-storage-account-output-property-names-are-lowercase?forum=AzureStreamAnalytics 

                newLocationJerkModel = new LocationJerkModel()
                {
                    Latitude = eventData.Latitude,
                    Longitude = eventData.Longitude,
                    Altitude = eventData.Altitude,
                    Status = LocationStatus.Caution,
                    DeviceList = null
                };

                string deviceId = eventData.DeviceId;

                if (deviceId != null)
                {
                    newLocationJerkModel.DeviceList = new List<DeviceJerkModel>()
                    {
                        new DeviceJerkModel()
                        {
                            DeviceId = deviceId,
                            Speed = eventData.Speed,
                            Heading = eventData.Heading,
                            CapturedJerks = eventData.Jerks
                        }
                    };

                }
                else
                {
                    Trace.TraceError("ActionProcessor: telemetryDeviceId value is empty");
                }

                if (eventData.Speed > 5)
                {
                    newLocationJerkModel.Status = GetLocationStatus(eventData);
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("ActionProcessor: exception in ProcessTelemetryJerk:");
                Trace.TraceError(e.ToString());
            }

            return newLocationJerkModel;
        }

        private void UpdateLocationJerkList(LocationJerkModel newLocationJerk, ref List<LocationJerkModel> newLocationJerkList)
        {
            if (newLocationJerk != null)
            {
                LocationJerkModel found = newLocationJerkList.FirstOrDefault(l => l.Latitude == newLocationJerk.Latitude && l.Longitude == newLocationJerk.Longitude);

                if (found == null)
                {
                    newLocationJerkList.Add(newLocationJerk);
                }
                else
                {
                    foreach (DeviceJerkModel deviceJerk in newLocationJerk.DeviceList)
                    {
                        DeviceJerkModel existingDeviceJerk = found.DeviceList.FirstOrDefault(d => d.DeviceId == deviceJerk.DeviceId);

                        if (existingDeviceJerk == null)
                        {
                            found.DeviceList.Add(deviceJerk);
                        }
                        else
                        {
                            existingDeviceJerk.CapturedJerks.AddRange(deviceJerk.CapturedJerks);
                        }
                    }
                }
            }            
        }

        private LocationStatus GetLocationStatus(TelemetryJerkModel eventData)
        {
            if (eventData.Speed >= 8 && eventData.Speed < 25)
            {
                var highestVerticalJerk = eventData.Jerks.Max(j => Math.Abs(j.VerticalJerk));
                var highestLateralJerk = eventData.Jerks.Max(j => Math.Abs(j.LateralJerk));

                if (highestVerticalJerk >= 25 || highestLateralJerk >= 30)
                {
                    return LocationStatus.Critical;
                }
            }
            else
            {
                if (eventData.Speed >= 25)
                {
                    var highestVerticalJerk = eventData.Jerks.Max(j => Math.Abs(j.VerticalJerk));
                    var highestLateralJerk = eventData.Jerks.Max(j => Math.Abs(j.LateralJerk));

                    if (highestVerticalJerk >= 30 || highestLateralJerk >= 34)
                    {
                        return LocationStatus.Critical;
                    }
                }
            }

            return LocationStatus.Caution;
        }

        private async Task SaveToLocationJerkInfo(List<LocationJerkModel> lstLocationJerks)
        {
            if (lstLocationJerks != null && lstLocationJerks.Count() > 0)
            {
                await _locationJerkLogic.SaveLocationJerkInfoAsync(lstLocationJerks);
            }
        }

        private async Task ProcessAction(ActionModel eventData)
        {
            if (eventData == null)
            {
                Trace.TraceWarning("Action event is null");
                return;
            }

            try
            {
                // NOTE: all column names from ASA come out as lowercase; see 
                // https://social.msdn.microsoft.com/Forums/office/en-US/c79a662b-5db1-4775-ba1a-23df1310091d/azure-table-storage-account-output-property-names-are-lowercase?forum=AzureStreamAnalytics 

                string deviceId = eventData.DeviceID;
                string ruleOutput = eventData.RuleOutput;

                if (ruleOutput.Equals("AlarmTemp", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("ProcessAction: temperature rule triggered!");
                    double tempReading = eventData.Reading;

                    string tempActionId = await _actionMappingLogic.GetActionIdFromRuleOutputAsync(ruleOutput);

                    if (!string.IsNullOrWhiteSpace(tempActionId))
                    {
                        await _actionLogic.ExecuteLogicAppAsync(
                        tempActionId,
                        deviceId,
                        "Temperature",
                        tempReading);
                    }
                    else
                    {
                        Trace.TraceError("ActionProcessor: tempActionId value is empty for temperatureRuleOutput '{0}'", ruleOutput);
                    }
                }

                if (ruleOutput.Equals("AlarmHumidity", StringComparison.OrdinalIgnoreCase))
                {
                    Trace.TraceInformation("ProcessAction: humidity rule triggered!");
                    double humidityReading = eventData.Reading;

                    string humidityActionId = await _actionMappingLogic.GetActionIdFromRuleOutputAsync(ruleOutput);

                    if (!string.IsNullOrWhiteSpace(humidityActionId))
                    {
                        await _actionLogic.ExecuteLogicAppAsync(
                            humidityActionId,
                            deviceId,
                            "Humidity",
                            humidityReading);
                    }
                    else
                    {
                        Trace.TraceError("ActionProcessor: humidityActionId value is empty for humidityRuleOutput '{0}'", ruleOutput);
                    }
                }
            }
            catch (Exception e)
            {
                Trace.TraceError("ActionProcessor: exception in ProcessAction:");
                Trace.TraceError(e.ToString());
            }
        }

        public Task CloseAsync(PartitionContext context, CloseReason reason)
        {
            Trace.TraceInformation("ActionProcessor: Close : Partition : " + context.Lease.PartitionId);
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
                    "{0}{0}*** CheckpointAsync Exception - ActionProcessor.CloseAsync ***{0}{0}{1}{0}{0}",
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
