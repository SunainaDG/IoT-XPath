using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class LocationRuleBlobEntity
    {
        public string Id { get; set; }
        public double RegionLatitude { get; set; }
        public double RegionLongitude { get; set; }
        public double Vertical { get; set; }
        public double Lateral { get; set; }
        public double Forward { get; set; }
        public string RuleOutput { get; set; }
    }
}
