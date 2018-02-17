using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface ILocationRulesRepository
    {
        Task<List<LocationRule>> GetAllRulesAsync();
        Task<LocationRule> GetLocationRuleAsync(string regionId, string ruleId);
        Task<List<LocationRule>> GetAllRulesForRegionAsync(string regionId);
        Task<TableStorageResponse<LocationRule>> SaveLocationRuleAsync(LocationRule updatedRule);
        Task<TableStorageResponse<LocationRule>> DeleteLocationRuleAsync(LocationRule ruleToDelete);
    }
}
