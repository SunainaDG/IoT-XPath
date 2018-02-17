using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public class TelemetryJerkComparer : IEqualityComparer<TelemetryJerkModel>
    {
        public bool Equals(TelemetryJerkModel x, TelemetryJerkModel y)
        {
            if (x == null || y == null)
                return false;

            var IsJerkListEqual = x.Jerks.All(j => y.Jerks.Contains(j,new JerkComparer()));

            return
                x.DeviceId == y.DeviceId &&
                x.PartionId == y.PartionId &&
                x.Altitude == y.Altitude &&
                x.ForwardThreshold == y.ForwardThreshold &&
                x.Heading == y.Heading &&                
                x.LateralThreshold == y.LateralThreshold &&
                x.Latitude == y.Latitude &&
                x.Longitude == y.Longitude &&
                x.RuleOutput == y.RuleOutput &&
                x.Speed == y.Speed &&
                x.VerticalThreshold == y.VerticalThreshold &&
                IsJerkListEqual;
        }

        public int GetHashCode(TelemetryJerkModel obj)
        {
            return obj.DeviceId.GetHashCode() ^
                obj.PartionId.GetHashCode() ^
                obj.Altitude.GetHashCode() ^
                obj.ForwardThreshold.GetHashCode() ^
                obj.Heading.GetHashCode() ^
                obj.LateralThreshold.GetHashCode() ^
                obj.Latitude.GetHashCode() ^
                obj.Longitude.GetHashCode() ^
                obj.RuleOutput.GetHashCode() ^
                obj.Speed.GetHashCode() ^
                obj.VerticalThreshold.GetHashCode();
        }
    }
}
