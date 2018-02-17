using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    /// <summary>
    /// A model that provides information about a single telemetry field
    /// </summary>
    public class LocationJerkGraphFieldModel
    {
        /// <summary>
        /// The user-friendly name for the field
        /// </summary>
        public string DisplayName
        {
            get;
            set;
        }

        /// <summary>
        /// The name of the field in storage
        /// </summary>
        public string Name
        {
            get;
            set;
        }

        /// <summary>
        /// The type of the field, such as "double" or "integer"
        /// </summary>
        public string Type
        {
            get;
            set;
        }
    }
}