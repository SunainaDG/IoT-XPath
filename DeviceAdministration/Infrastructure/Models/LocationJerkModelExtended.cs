using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class LocationJerkModelExtended : LocationJerkModel
    {
        public int NoOfDevices { get; set; }
    }
}
