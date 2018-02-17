using System.Collections.Generic;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    /// <summary>
    /// A model that represents a Location's jerk informations.
    /// </summary>
    public class LocationJerkModel
    {
        /// <summary>
        /// Gets or sets the Latitude of the Location.
        /// </summary>
        public double? Latitude
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Longitude of the Location.
        /// </summary>
        public double? Longitude
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Longitude of the Location.
        /// </summary>
        public double? Altitude
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the Status of the Location.
        /// </summary>
        public LocationStatus Status
        {
            get;
            set;
        }

        public List<DeviceJerkModel> DeviceList{ get; set; }
    }

    public enum LocationStatus
    {
        Caution = 1,
        Critical
    }
}
