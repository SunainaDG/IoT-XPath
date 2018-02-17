using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;


namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface ILocationRulesLogic
    {
        Task<List<LocationRule>> GetAllRulesAsync();
        Task<LocationRule> GetLocationRuleOrDefaultAsync(string regionId, double latitude, double longitude);
        Task<LocationRule> GetDefaultLocationRuleAsync();
        Task<LocationRule> GetLocationRuleAsync(string regionId, string ruleId);
        Task<TableStorageResponse<LocationRule>> SaveLocationRuleAsync(LocationRule updatedRule);
        Task<LocationRule> GetNewRuleAsync(string regionId,double latitude, double longitude);
        Task<TableStorageResponse<LocationRule>> UpdateLocationRuleEnabledStateAsync(string regionId, string ruleId, bool enabled);
        Task<TableStorageResponse<LocationRule>> DeleteLocationRuleAsync(string regionId, string ruleId);
    }
}
