using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class FeedbackModel
    {
        public CurrentUserLocation CurrentUserLocation { get; set; }
        public List<LocationModel> NearestJerks { get; set; }

        public FeedbackModel(CurrentDeviceLocationModel deviceLocation = null)
        {
            if (deviceLocation != null)
            {
                CurrentUserLocation = new CurrentUserLocation()
                {
                    DeviceId = deviceLocation.DeviceId,
                    Latitude = deviceLocation.Latitude,
                    Longitude = deviceLocation.Longitude,
                    Altitude = deviceLocation.Altitude,
                    Heading = deviceLocation.Heading
                };
            }
            
            NearestJerks = new List<LocationModel>();
        }
    }
}
