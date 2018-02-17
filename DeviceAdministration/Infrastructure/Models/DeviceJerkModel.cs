using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class DeviceJerkModel
    {
        /// <summary>
        /// Gets or Sets the DeviceID from which the jerk has been captured.
        /// </summary>
        public string DeviceId { get; set; }

        /// <summary>
        /// Gets or Sets the Speed of the Device when the jerk was captured.
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Gets or Sets the direction in which the Device/Vehicle 
        /// was moving when the jerk was captured.
        /// </summary>
        public double Heading { get; set; }

        public List<JerkModel> CapturedJerks { get; set; }
    }
}
