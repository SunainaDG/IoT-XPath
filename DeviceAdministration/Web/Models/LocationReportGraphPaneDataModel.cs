using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    /// <summary>
    /// A model for holding data that the Location Report Graph pane shows.
    /// </summary>
    public class LocationReportGraphPaneDataModel
    {
        /// <summary>
        /// Gets or Sets the DeviceID from which the jerk has been captured.
        /// </summary>
        public string DeviceId
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or Sets the Speed of the Device when the jerk was captured.
        /// </summary>
        public double Speed { get; set; }

        /// <summary>
        /// Gets or Sets the direction in which the Device/Vehicle 
        /// was moving when the jerk was captured.
        /// </summary>
        public double Heading { get; set; }

        /// <summary>
        /// Gets or sets an array of LocationJerkGraphModel for backing the 
        /// telemetry line graph.
        /// </summary>
        public LocationJerkGraphModel[] LocationJerkGraphModels
        {
            get;
            set;
        }

        public LocationJerkGraphFieldModel[] LocationJerkGraphFields
        {
            get;
            set;
        }
    }
}