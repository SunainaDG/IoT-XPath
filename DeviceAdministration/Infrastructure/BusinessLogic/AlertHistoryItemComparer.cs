using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public class AlertHistoryItemComparer : IEqualityComparer<AlertHistoryItemModel>
    {
        public bool Equals(AlertHistoryItemModel x, AlertHistoryItemModel y)
        {
            return x.DeviceId == y.DeviceId &&
                x.Lateral == y.Lateral &&
                x.Vertical == y.Vertical &&
                x.RuleOutput == y.RuleOutput &&
                x.Timestamp == y.Timestamp;
        }

        public int GetHashCode(AlertHistoryItemModel obj)
        {
            return obj.DeviceId.GetHashCode() ^
                obj.Lateral.GetHashCode() ^
                obj.Vertical.GetHashCode() ^
                obj.RuleOutput.GetHashCode() ^
                obj.Timestamp.GetHashCode();
        }
    }
}
