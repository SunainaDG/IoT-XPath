using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class LocationJerkGraphModel
    {
        /// <summary>
        /// Values for graph data associated with individual fields
        /// </summary>
        private IDictionary<string, double> values = new Dictionary<string, double>();
        public IDictionary<string, double> Values
        {
            get { return values; }
            set { values = value; }
        }

        /// <summary>
        /// Gets or sets the time of record for the represented telemetry 
        /// recording.
        /// </summary>
        public DateTime? Timestamp
        {
            get;
            set;
        }
    }
}