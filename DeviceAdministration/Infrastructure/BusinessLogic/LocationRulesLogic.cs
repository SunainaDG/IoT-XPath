using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    /// <summary>
    /// Logic class for retrieving, manipulating and persisting Location Rules
    /// </summary>
    public class LocationRulesLogic : ILocationRulesLogic
    {
        private readonly ILocationRulesRepository _locationRulesRepository;
        private readonly IActionMappingLogic _actionMappingLogic;

        public LocationRulesLogic(ILocationRulesRepository locationRulesRepository, IActionMappingLogic actionMappingLogic)
        {
            _locationRulesRepository = locationRulesRepository;
            _actionMappingLogic = actionMappingLogic;
        }

        /// <summary>
        /// Retrieve the full list of Location Rules
        /// </summary>
        /// <returns></returns>
        public async Task<List<LocationRule>> GetAllRulesAsync()
        {
            return await _locationRulesRepository.GetAllRulesAsync();
        }

        /// <summary>
        /// Retrieve an existing rule for editing. If none is found then a default, bare-bones rule is returned for creating new
        /// A new rule is not persisted until it is saved. Distinct Rules are defined by the combination key of regionID and ruleId
        /// 
        /// Use this method if you are not sure the desired rule exists
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public async Task<LocationRule> GetLocationRuleOrDefaultAsync(string regionId, double latitude, double longitude)
        {
            List<LocationRule> rulesForRegion = await _locationRulesRepository.GetAllRulesForRegionAsync(regionId);
            foreach (LocationRule rule in rulesForRegion)
            {
                if (rule.RegionLatitude == latitude && rule.RegionLongitude == longitude)
                {
                    return rule;
                }
            }

            var createdRule = await GetNewRuleAsync(regionId, latitude,longitude);
            return createdRule;      
        }

        /// <summary>
        /// Retrieve the default location rule.
        /// </summary>
        /// <returns></returns>
        public async Task<LocationRule> GetDefaultLocationRuleAsync()
        {
            LocationRule defaultRule = await GetLocationRuleAsync("default","default");

            if (defaultRule == null)
            {
                defaultRule = new LocationRule("default")
                {
                    RegionId = "default",
                    Region = "All",
                    RegionLatitude = 300.0,
                    RegionLongitude = 300.0,
                    VerticalThreshold = 25.05,
                    LateralThreshold = 20.0,
                    ForwardThreshold = 15.8,
                    RuleOutput = "DefaultJerk",
                    Etag = "",
                    EnabledState = true
                };

                await SaveLocationRuleAsync(defaultRule);
            }

            return defaultRule;
        }

        /// <summary>
        /// Retrieve an existing rule for a region/ruleId pair. If a rule does not exist
        /// it will return null. This method is best used when you know the rule exists.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public async Task<LocationRule> GetLocationRuleAsync(string regionId, string ruleId)
        {
            return await _locationRulesRepository.GetLocationRuleAsync(regionId, ruleId);
        }

        /// <summary>
        /// Save a rule to the data store. This method should be used for new rules as well as updating existing rules
        /// </summary>
        /// <param name="newRule"></param>
        /// <returns></returns>
        public async Task<TableStorageResponse<LocationRule>> SaveLocationRuleAsync(LocationRule newRule)
        {
            if (newRule.RegionId == "default")
            {
                return await _locationRulesRepository.SaveLocationRuleAsync(newRule);
            }

            string regionId = $"{Math.Truncate(newRule.RegionLatitude * 10)/10}_{Math.Truncate(newRule.RegionLongitude *10)/10}";

            if (newRule.RegionId != regionId || string.IsNullOrWhiteSpace(newRule.RuleId))
            {
                var response = new TableStorageResponse<LocationRule>();
                response.Entity = newRule;
                response.Status = TableStorageResponseStatus.IncorrectEntry;

                return response;
            }

            //Enforce single instance of a rule for a data field for a given device
            List<LocationRule> foundForRegion = await _locationRulesRepository.GetAllRulesForRegionAsync(newRule.RegionId);
            foreach (LocationRule rule in foundForRegion)
            {
                if ((rule.RuleId != newRule.RuleId) &&
                    (rule.RegionLatitude == newRule.RegionLatitude) && (rule.RegionLongitude == newRule.RegionLongitude))
                {
                    var response = new TableStorageResponse<LocationRule>();
                    response.Entity = rule;
                    response.Status = TableStorageResponseStatus.DuplicateInsert;

                    return response;
                }
            }

            return await _locationRulesRepository.SaveLocationRuleAsync(newRule);
        }

        /// <summary>
        /// Generate a new rule with bare-bones configuration. This new rule can then be configured and sent
        /// back through the SaveDeviceRuleAsync method to persist.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public async Task<LocationRule> GetNewRuleAsync(string regionId, double latitude, double longitude)
        {
            //making sure we have only 2 decimal places counted
            double regionLatitude = Math.Truncate(latitude * 100) / 100;
            double regionLongitude = Math.Truncate(longitude * 100) / 100;

            return await Task.Run(() =>
            {
                var rule = new LocationRule();
                rule.InitializeNewRule(regionId, regionLatitude, regionLongitude);

                return rule;
            });
        }

        /// <summary>
        /// Updated the enabled state of a given rule. This method does not update any other properties on the rule
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="ruleId"></param>
        /// <param name="enabled"></param>
        /// <returns></returns>
        public async Task<TableStorageResponse<LocationRule>> UpdateLocationRuleEnabledStateAsync(string regionId, string ruleId, bool enabled)
        {
            LocationRule found = await _locationRulesRepository.GetLocationRuleAsync(regionId, ruleId);
            if (found == null)
            {
                var response = new TableStorageResponse<LocationRule>();
                response.Entity = found;
                response.Status = TableStorageResponseStatus.NotFound;

                return response;
            }

            found.EnabledState = enabled;

            return await _locationRulesRepository.SaveLocationRuleAsync(found);
        }

        public async Task<TableStorageResponse<LocationRule>> DeleteLocationRuleAsync(string regionId, string ruleId)
        {
            LocationRule found = await _locationRulesRepository.GetLocationRuleAsync(regionId, ruleId);
            if (found == null)
            {
                var response = new TableStorageResponse<LocationRule>();
                response.Entity = found;
                response.Status = TableStorageResponseStatus.NotFound;

                return response;
            }

            return await _locationRulesRepository.DeleteLocationRuleAsync(found);
        }
    }
}
