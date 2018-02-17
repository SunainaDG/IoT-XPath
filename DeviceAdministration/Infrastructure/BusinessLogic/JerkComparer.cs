using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public class JerkComparer : IEqualityComparer<JerkModel>
    {
        public bool Equals(JerkModel x, JerkModel y)
        {
            return x.ForwardJerk == y.ForwardJerk &&
                x.JerkTimeStamp == y.JerkTimeStamp &&
                x.LateralJerk == y.LateralJerk &&
                x.VerticalJerk == y.VerticalJerk;
        }

        public int GetHashCode(JerkModel obj)
        {
            return obj.ForwardJerk.GetHashCode() ^
                obj.JerkTimeStamp.GetHashCode() ^
                obj.LateralJerk.GetHashCode() ^
                obj.VerticalJerk.GetHashCode();
        }
    }
}
