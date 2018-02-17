using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GlobalResources;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    public class LocationRulesController : Controller
    {
        private readonly ILocationRulesLogic _locationRulesLogic;

        public LocationRulesController(ILocationRulesLogic locationRulesLogic)
        {
            this._locationRulesLogic = locationRulesLogic;
        }

        // GET: LocationRules
        [RequirePermission(Permission.ViewRules)]        
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Return a view for the right panel on the DeviceRules index screen
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        [HttpGet]
        [RequirePermission(Permission.ViewRules)]
        public async Task<ActionResult> GetRuleProperties(string regionId, string ruleId)
        {
            LocationRule rule = await _locationRulesLogic.GetLocationRuleAsync(regionId, ruleId);
            EditLocationRuleModel editModel = CreateEditModelFromLocationRule(rule);
            editModel.IsCreateRequest = false;
            return PartialView("_LocationRuleProperties", editModel);
        }

        /// <summary>
        /// Navigate to the EditDefaultRule screen
        /// </summary>
        /// <returns></returns>
        [RequirePermission(Permission.ViewRules)]
        public async Task<ActionResult> Default()
        {
            LocationRule rule = await _locationRulesLogic.GetDefaultLocationRuleAsync();
            EditLocationRuleModel editModel = CreateEditModelFromLocationRule(rule);
            editModel.IsCreateRequest = false;

            return View("EditLocationRuleProperties", editModel);
        }

        /// <summary>
        /// Navigate to the Create or Update screen
        /// </summary>
        /// <returns></returns>
        [RequirePermission(Permission.CreateRules)]
        public async Task<ActionResult> CreateOrUpdate(double lat, double lng)
        {
            var regionId = $"{Math.Truncate(lat * 10)/10}_{Math.Truncate(lng*10)/10}";
            LocationRule rule = await _locationRulesLogic.GetLocationRuleOrDefaultAsync(regionId, lat, lng);
            EditLocationRuleModel editModel = CreateEditModelFromLocationRule(rule);

            if (string.IsNullOrWhiteSpace(rule.RuleId))
            {
                editModel.IsCreateRequest = true;
            }
            else
            {
                editModel.IsCreateRequest = false;
            }

            return View("EditLocationRuleProperties", editModel);
        }

        /// <summary>
        /// Navigate to the EditRule screen
        /// </summary>
        /// <returns></returns>
        [RequirePermission(Permission.EditRules)]
        public async Task<ActionResult> Edit(string regionId, string ruleId)
        {            
            LocationRule rule = await _locationRulesLogic.GetLocationRuleAsync(regionId, ruleId);
            EditLocationRuleModel editModel = CreateEditModelFromLocationRule(rule);
            editModel.IsCreateRequest = false;

            return View("EditLocationRuleProperties", editModel);
        }

        /// <summary>
        /// Save changes to database (both table and blob) and 
        /// navigate to the Rules Index screen
        /// </summary>
        /// <returns></returns>

        [RequirePermission(Permission.EditRules)]
        public async Task<ActionResult> UpdateRuleProperties(EditLocationRuleModel editModel)
        {
            LocationRule updatedRule = CreateLocationRuleFromEditModel(editModel);
            TableStorageResponse<LocationRule> result = await _locationRulesLogic.SaveLocationRuleAsync(updatedRule);

            return BuildRuleUpdateResponse(result);
        }

        /// <summary>
        /// Update the enabled state for a rule. No other properties will be updated on the rule, 
        /// even if they are included in the device rule model. This method return json with either 
        /// success = true or error = errorMessage. If the user wants to update the ui with fresh
        /// data subsequent explicit calls should be made for new data
        /// </summary>
        /// <param name="ruleModel"></param>
        /// <returns></returns>
        [HttpPost]
        [RequirePermission(Permission.EditRules)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> UpdateRuleEnabledState(EditLocationRuleModel ruleModel)
        {
            TableStorageResponse<LocationRule> response = await _locationRulesLogic.UpdateLocationRuleEnabledStateAsync(
                ruleModel.RegionId,
                ruleModel.RuleId,
                ruleModel.EnabledState);

            return BuildRuleUpdateResponse(response);
        }

        /// <summary>
        /// Delete the given rule for a device
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        [HttpDelete]
        [RequirePermission(Permission.DeleteRules)]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> DeleteLocationRule(string regionId, string ruleId)
        {
            TableStorageResponse<LocationRule> response = await _locationRulesLogic.DeleteLocationRuleAsync(regionId, ruleId);
            return BuildRuleUpdateResponse(response);
        }

        private EditLocationRuleModel CreateEditModelFromLocationRule(LocationRule rule)
        {
            EditLocationRuleModel model = new EditLocationRuleModel()
            {
                RegionId = rule.RegionId,
                Region = rule.Region,
                RuleId = rule.RuleId,
                RegionLatitude = rule.RegionLatitude,
                RegionLongitude = rule.RegionLongitude,
                VerticalThreshold = rule.VerticalThreshold,
                LateralThreshold = rule.LateralThreshold,
                ForwardThreshold = rule.ForwardThreshold,
                EnabledState = rule.EnabledState,
                RuleOutput = rule.RuleOutput,
                Etag = rule.Etag
            };

            return model;
        }

        private LocationRule CreateLocationRuleFromEditModel(EditLocationRuleModel editModel)
        {
            LocationRule rule = new LocationRule()
            {
                EnabledState = editModel.EnabledState,
                RegionId = editModel.RegionId,
                RegionLatitude = editModel.RegionLatitude,
                RegionLongitude = editModel.RegionLongitude,
                Region = editModel.Region,
                RuleId = editModel.RuleId,
                VerticalThreshold = editModel.VerticalThreshold,
                LateralThreshold = editModel.LateralThreshold,
                ForwardThreshold = editModel.ForwardThreshold,
                RuleOutput = editModel.RuleOutput,
                Etag = editModel.Etag
            };

            return rule;
        }

        private JsonResult BuildRuleUpdateResponse(TableStorageResponse<LocationRule> response)
        {
            switch (response.Status)
            {
                case TableStorageResponseStatus.Successful:
                    return Json(new
                    {
                        success = true
                    });
                case TableStorageResponseStatus.ConflictError:
                    return Json(new
                    {
                        error = Strings.TableDataSaveConflictErrorMessage,
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
                case TableStorageResponseStatus.DuplicateInsert:
                    return Json(new
                    {
                        error = Strings.RuleAlreadyAddedError,
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
                case TableStorageResponseStatus.NotFound:
                    return Json(new
                    {
                        error = Strings.UnableToRetrieveRuleFromService,
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
                case TableStorageResponseStatus.IncorrectEntry:
                    return Json(new
                    {
                        //get detailed error for incorrect entry
                        error = GetIncorrectInputDetails(response.Entity),
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
                case TableStorageResponseStatus.UnknownError:
                default:
                    return Json(new
                    {
                        error = Strings.RuleUpdateError,
                        entity = JsonConvert.SerializeObject(response.Entity)
                    });
            }
        }

        private string GetIncorrectInputDetails(LocationRule rule)
        {
            if (string.IsNullOrWhiteSpace(rule.RuleId))
            {
                return Strings.EmptyRuleId;
            }
            return Strings.IncorrectRegionID;
        }
    }
}