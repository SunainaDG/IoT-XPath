using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;


namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Controllers
{
    public class LocationController : Controller
    {
        private readonly ILocationJerkLogic _locationJerkLogic;
        private readonly ILocationRulesLogic _locationRulesLogic;

        public LocationController(ILocationJerkLogic locationJerkLogic, ILocationRulesLogic locationRulesLogic)
        {
            this._locationJerkLogic = locationJerkLogic;
            this._locationRulesLogic = locationRulesLogic;
        }

        [RequirePermission(Permission.ViewLocations)]
        public ActionResult Index(double? latitude, double? longitude)
        {
            LocationPropertiesModel model = new LocationPropertiesModel();
            if (latitude != null && longitude != null)
            {
                model.FromMap = true;
                model.Latitude = latitude;
                model.Longitude = longitude;
                return View(model);
            }
            
            model.FromMap = false;
            return View(model);            
        }

        /// <summary>
        /// Return a view for the right panel on the Locations index screen
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns>Location Properties View</returns>
        [HttpGet]
        [RequirePermission(Permission.ViewLocations)]
        public async Task<ActionResult> GetLocationProperties(double? latitude, double? longitude)
        {
            LocationJerkModelExtended location = await _locationJerkLogic.GetLocationDetails(latitude, longitude);
            LocationPropertiesModel locationModel = CreateViewModelFromLocationJerk(location);
            return PartialView("_LocationProperties", locationModel);
        }

        // <summary>
        /// Navigate to the LocationReport screen
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>

        public async Task<ActionResult> LocationReport(LocationPropertiesModel locationProperties)
        {
            var errorMessage = locationProperties.CheckForErrorMessage();

            if (!String.IsNullOrWhiteSpace(errorMessage))
            {
                return Json(new { error = errorMessage });
            }

            var regionLatitude = Math.Truncate((double)locationProperties.Latitude * 100)/100;
            var regionLongitude = Math.Truncate((double)locationProperties.Longitude * 100)/100;
            var regionId = $"{Math.Truncate(regionLatitude * 10)/10}_{Math.Truncate(regionLongitude * 10)/10}";

            LocationRule rule = await _locationRulesLogic.GetLocationRuleOrDefaultAsync(regionId,regionLatitude,regionLongitude);

            locationProperties.RuleId = rule.RuleId;
            locationProperties.RegionLatitude = rule.RegionLatitude;
            locationProperties.RegionLongitude = rule.RegionLongitude;
            locationProperties.JerkedDeviceSelectList = GetSelectListFromJsonList(locationProperties.JsonList);
            return View("LocationReport", locationProperties);
        }

        public async Task<ActionResult> DeleteLocation(LocationPropertiesModel locationProperties)
        {
            if (locationProperties.Latitude != null && locationProperties.Longitude != null)
            {
                await _locationJerkLogic.DeleteLocationJerkAsync(locationProperties.Latitude, locationProperties.Longitude);
            }

            locationProperties = new LocationPropertiesModel();
            locationProperties.FromMap = false;
            return View("Index",locationProperties);
        }

        [HttpPost]
        public async Task<ActionResult> DeleteSelected(IEnumerable<LocationReferenceModel> locations)
        {
            if (locations != null)
            {
                try
                {
                    IEnumerable<LocationModel> locationBatch = GetLocationModelListFromReference(locations);

                    await _locationJerkLogic.DeleteLocationsInBatchAsync(locationBatch);
                }
                catch (Exception ex)
                {

                }
                
            }

            LocationPropertiesModel locationProperties = new LocationPropertiesModel();
            locationProperties.FromMap = false;
            return View("Index", locationProperties);
        }

        private LocationPropertiesModel CreateViewModelFromLocationJerk(LocationJerkModelExtended locationJerkModel)
        {
            LocationPropertiesModel model = new LocationPropertiesModel();
            model.JsonList = JsonConvert.SerializeObject(locationJerkModel.DeviceList);
            model.NoOfDevices = locationJerkModel.NoOfDevices;
            model.Latitude = locationJerkModel.Latitude;
            model.Longitude = locationJerkModel.Longitude;
            model.Altitude = locationJerkModel.Altitude;

            //if (locationJerkModel.Latitude != null)
            //{
            //    model.Latitude = locationJerkModel.Latitude.ToString();
            //}

            //if (locationJerkModel.Longitude != null)
            //{
            //    model.Longitude = locationJerkModel.Longitude.ToString();
            //}

            //if (locationJerkModel.Altitude != null)
            //{
            //    model.Altitude = locationJerkModel.Altitude.ToString();
            //}

            switch (locationJerkModel.Status)
            {
                case LocationStatus.Critical:
                    model.Status = "Road Condition Critical.";
                    break;
                case LocationStatus.Caution:
                    model.Status = "Bad Road, proceed with Caution.";
                    break;
                default:
                    model.Status = "Road Fixed";
                    break;
            }

            return model;

        }

        public List<SelectListItem> GetSelectListFromJsonList(string deviceList)
        {
            List<DeviceJerkModel> list = JsonConvert.DeserializeObject<List<DeviceJerkModel>>(deviceList);

            List<SelectListItem> result = new List<SelectListItem>();

            if (deviceList != null && list.Count > 0)
            {
                foreach (DeviceJerkModel device in list)
                {
                    SelectListItem item = new SelectListItem
                    {
                        Text = device.DeviceId,
                        Value = device.DeviceId
                    };

                    result.Add(item);
                }
            }

            return result;
        }

        private IEnumerable<LocationModel> GetLocationModelListFromReference(IEnumerable<LocationReferenceModel> locations)
        {
            List<LocationModel> results = new List<LocationModel>();
            LocationModel model = null;

            if (locations != null)
            {
                foreach (LocationReferenceModel reference in locations)
                {
                    model = new LocationModel()
                    {
                        Latitude = reference.Latitude,
                        Longitude = reference.Longitude
                    };

                    results.Add(model);
                }
            }

            return results.AsEnumerable();
        }

    }
}