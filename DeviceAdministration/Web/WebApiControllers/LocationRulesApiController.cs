using System;
using System.Collections.Generic;
using GlobalResources;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using System.Web.Http;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/locationrules")]
    public class LocationRulesApiController : WebApiControllerBase
    {
        private readonly ILocationRulesLogic _locationRulesLogic;

        public LocationRulesApiController(ILocationRulesLogic locationRulesLogic)
        {
            this._locationRulesLogic = locationRulesLogic;
        }

        // GET: api/v1/locationrules
        //
        // This endpoint is used for apps and other platforms to get a list of device rules (whereas endpoint below is used by jQuery DataTables grid)
        //
        // See, for example: http://stackoverflow.com/questions/9981330/how-to-pass-an-array-of-integers-to-a-asp-net-web-api-rest-service
        // Example: api/v1/locationrules
        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewRules)]
        public async Task<HttpResponseMessage> GetLocationRulesAsync()
        {
            return await GetServiceResponseAsync(async () => (await _locationRulesLogic.GetAllRulesAsync()));
        }

        // POST: api/v1/locationrules/list
        // This endpoint is used by the jQuery DataTables grid to get data (and accepts an unusual data format based on that grid)
        [HttpPost]
        [Route("list")]
        [WebApiRequirePermission(Permission.ViewRules)]
        public async Task<HttpResponseMessage> GetLocationRulesAsDataTablesResponseAsync()
        {
            return await GetServiceResponseAsync<DataTablesResponse<LocationRule>>(async () =>
            {
                var queryResult = await _locationRulesLogic.GetAllRulesAsync();

                queryResult = queryResult.Where(r => r.RegionId != Strings.DefaultRuleID).ToList();

                var dataTablesResponse = new DataTablesResponse<LocationRule>()
                {
                    RecordsTotal = queryResult.Count,
                    RecordsFiltered = queryResult.Count,
                    Data = queryResult.ToArray()
                };

                return dataTablesResponse;

            }, false);
        }

        // POST: api/v1/locationrules/countrystatelist
        // This endpoint is used by the jQuery DataTables grid to get data (and accepts an unusual data format based on that grid)
        [HttpGet]
        [Route("countrystatelist")]
        public async Task<HttpResponseMessage> GetCountriesAndStatesForDropDown()
        {
            return await GetServiceResponseAsync<CountryStates>(async () => 
            {
                return await Task.Run(()=>{
                    CountryStates countryStatesList = new CountryStates();
                    return countryStatesList;
                });
            },false);
        }
    }
}
