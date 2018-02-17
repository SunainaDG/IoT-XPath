using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class TelemetryJerkModel
    {
        private const string DEVICE_ID_COLUMN_NAME = "deviceid";
        private const string PARTITION_ID_COLUMN_NAME = "partitionid";
        private const string LATITUDE_COLUMN_NAME = "latitude";
        private const string LONGITUDE_COLUMN_NAME = "longitude";
        private const string ALTITUDE_COLUMN_NAME = "altitude";
        private const string CARSPEED_COLUMN_NAME = "carspeed";
        private const string HEADING_COLUMN_NAME = "headingtowards";
        private const string VERTICAL_THRESHOLD_COLUMN_NAME = "verticalthreshold";
        private const string LATERAL_THRESHOLD_COLUMN_NAME = "lateralthreshold";
        private const string FORWARD_THRESHOLD_COLUMN_NAME = "forwardthreshold";
        private const string RULE_OUTPUT_COLUMN_NAME = "ruleoutput";
        private const string JERKS_COLUMN_NAME = "jerks";


        /// <summary>
        /// Gets or Sets the DeviceID from which the jerk has been captured.
        /// </summary>
        [JsonProperty(PropertyName = DEVICE_ID_COLUMN_NAME)]
        public string DeviceId { get; set; }

        /// <summary>
        /// Gets or Sets the PartitionID from which the jerk has been captured.
        /// </summary>
        [JsonProperty(PropertyName = PARTITION_ID_COLUMN_NAME)]
        public int PartionId { get; set; }

        /// <summary>
        /// Gets or sets the Latitude of the Location.
        /// </summary>
        [JsonProperty(PropertyName = LATITUDE_COLUMN_NAME)]
        public double? Latitude { get; set; }

        /// <summary>
        /// Gets or sets the Longitude of the Location.
        /// </summary>
        [JsonProperty(PropertyName = LONGITUDE_COLUMN_NAME)]
        public double? Longitude { get; set; }

        /// <summary>
        /// Gets or sets the Longitude of the Location.
        /// </summary>
        [JsonProperty(PropertyName = ALTITUDE_COLUMN_NAME)]
        public double? Altitude { get; set; }

        /// <summary>
        /// Gets or Sets the Speed of the Device when the jerk was captured.
        /// </summary>
        [JsonProperty(PropertyName = CARSPEED_COLUMN_NAME)]
        public double Speed { get; set; }

        /// <summary>
        /// Gets or Sets the direction in which the Device/Vehicle 
        /// was moving when the jerk was captured.
        /// </summary>
        [JsonProperty(PropertyName = HEADING_COLUMN_NAME)]
        public double Heading { get; set; }

        /// <summary>
        /// Gets or Sets the Vertical Jerk captured by the device.
        /// </summary>
        [JsonProperty(PropertyName = VERTICAL_THRESHOLD_COLUMN_NAME)]
        public double VerticalThreshold { get; set; }

        /// <summary>
        /// Gets or Sets the Lateral Jerk captured by the device.
        /// </summary>
        [JsonProperty(PropertyName = LATERAL_THRESHOLD_COLUMN_NAME)]
        public double LateralThreshold { get; set; }

        /// <summary>
        /// Gets or Sets the Forward Jerk captured by the device.
        /// </summary>
        [JsonProperty(PropertyName = FORWARD_THRESHOLD_COLUMN_NAME)]
        public double ForwardThreshold { get; set; }

        /// <summary>
        /// Gets or Sets the Action of the RuleOutput.
        /// </summary>
        [JsonProperty(PropertyName = RULE_OUTPUT_COLUMN_NAME)]
        public string RuleOutput { get; set; }

        /// <summary>
        /// Gets or Sets the List of Jerks Captured.
        /// </summary>
        [JsonProperty(PropertyName = JERKS_COLUMN_NAME)]
        public List<JerkModel> Jerks { get; set; }
    }
}
