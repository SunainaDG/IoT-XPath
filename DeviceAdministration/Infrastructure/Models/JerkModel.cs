using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// A model that represents a Jerk informations captured by a specific device at a specific location.
    /// </summary>
    public class JerkModel
    {
        private const string VERTICALJERK_COLUMN_NAME = "verticaljerk";
        private const string LATERALJERK_COLUMN_NAME = "lateraljerk";
        private const string FORWARDJERK_COLUMN_NAME = "forwardjerk";
        private const string TIMESTAMP_COLUMN_NAME = "jerktimestamp";


        /// <summary>
        /// Gets or Sets the Vertical Jerk captured by the device.
        /// </summary>
        [JsonProperty(PropertyName = VERTICALJERK_COLUMN_NAME)]
        public double VerticalJerk { get; set; }

        /// <summary>
        /// Gets or Sets the Lateral Jerk captured by the device.
        /// </summary>
        [JsonProperty(PropertyName = LATERALJERK_COLUMN_NAME)]
        public double LateralJerk { get; set; }

        /// <summary>
        /// Gets or Sets the Forward Jerk captured by the device.
        /// </summary>
        [JsonProperty(PropertyName = FORWARDJERK_COLUMN_NAME)]
        public double ForwardJerk { get; set; }

        /// <summary>
        /// Gets or Sets the time at which the jerk was captured.
        /// </summary>
        [JsonProperty(PropertyName = TIMESTAMP_COLUMN_NAME)]
        public DateTime? JerkTimeStamp { get; set; }
    }
}
