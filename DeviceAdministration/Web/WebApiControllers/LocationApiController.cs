using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Linq;
using System.Web;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.DataTables;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Security;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.WebApiControllers
{
    [RoutePrefix("api/v1/locations")]
    public class LocationApiController : WebApiControllerBase
    {
        private ILocationJerkLogic _locationJerkLogic;

        public LocationApiController(ILocationJerkLogic locationJerkLogic)
        {
            this._locationJerkLogic = locationJerkLogic;
        }

        [HttpGet]
        [Route("")]
        [WebApiRequirePermission(Permission.ViewLocations)]
        public async Task<HttpResponseMessage> GetLocationsAsync()
        {
            return await GetServiceResponseAsync(async () => (await _locationJerkLogic.LoadLatestLocationJerkInfoAsync()));
        }

        [HttpPost]
        [Route("list")]
        [WebApiRequirePermission(Permission.ViewLocations)]
        public async Task<HttpResponseMessage> GetLocationsAsDataTablesResponseAsync()
        {
            return await GetServiceResponseAsync<DataTablesResponse<LocationJerkModelExtended>>(async () =>
            {
                IEnumerable<LocationJerkModel> fetchedData = await _locationJerkLogic.LoadLatestLocationJerkInfoAsync();
                List<LocationJerkModelExtended> queryResult = fetchedData.Where(e => e.DeviceList != null).Select(e =>
                new LocationJerkModelExtended
                {
                    Latitude = e.Latitude,
                    Longitude = e.Longitude,
                    Altitude = e.Altitude,
                    Status = e.Status,
                    NoOfDevices = e.DeviceList.Count
                }).Where(a => a.NoOfDevices > 0).ToList();

                var dataTablesResponse = new DataTablesResponse<LocationJerkModelExtended>()
                {
                    RecordsTotal = queryResult.Count,
                    RecordsFiltered = queryResult.Count,
                    Data = queryResult.ToArray()
                };

                return dataTablesResponse;
            }, false);
        }

        [HttpGet]
        [Route("jerkgraph/{deviceId}/{latitude}/{longitude}/")]
        [WebApiRequirePermission(Permission.ViewLocations)]
        public async Task<HttpResponseMessage> GetJerkGraphData(string deviceId, string latitude, string longitude)
        {
            return await GetServiceResponseAsync<LocationReportGraphPaneDataModel>(async () =>
            {
                double lat;
                double lng;
                LocationReportGraphPaneDataModel dataModel = null;                
                if (Double.TryParse(latitude,out lat) && Double.TryParse(longitude, out lng) && !String.IsNullOrEmpty(deviceId))
                {
                    LocationJerkModelExtended locationJerkModel = await _locationJerkLogic.GetLocationDetails(lat as double?, lng as double?);
                    if (locationJerkModel.DeviceList != null)
                    {
                        DeviceJerkModel jerkModel = locationJerkModel.DeviceList.Where(d => d.DeviceId == deviceId).FirstOrDefault();

                        dataModel = new LocationReportGraphPaneDataModel()
                        {
                            DeviceId = jerkModel.DeviceId,
                            Speed = jerkModel.Speed,
                            Heading = jerkModel.Heading
                        };

                        IList<LocationJerkGraphFieldModel> graphFields = ExtractLocationJerkGraphFields();
                        dataModel.LocationJerkGraphFields = graphFields != null ? graphFields.ToArray() : null;

                        IEnumerable<LocationJerkGraphModel> graphModels = GetLocationJerkGraphModels(jerkModel.CapturedJerks);

                        if (graphModels == null)
                        {
                            dataModel.LocationJerkGraphModels = new LocationJerkGraphModel[0];
                        }
                        else
                        {
                            dataModel.LocationJerkGraphModels = graphModels.OrderBy(t => t.Timestamp).ToArray();
                        }
                    }
                }
                return dataModel;
            }, false);            
        }

        // GET api/<controller>/5
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<controller>
        public void Post([FromBody]string value)
        {
        }

        // PUT api/<controller>/5
        public void Put(int id, [FromBody]string value)
        {
        }

        [HttpPost]
        [Route("delete/all")]
        [WebApiRequirePermission(Permission.DeleteLocations)]
        public async Task<HttpResponseMessage> Delete()
        {
            return await GetServiceResponseAsync<bool>(async () =>
            {
                bool result = await _locationJerkLogic.DeleteAllLocationJerksAsync();
                
                return result;
            }, false);
        }

        private IList<LocationJerkGraphFieldModel> ExtractLocationJerkGraphFields()
        {
            var jerkGraphFields = new List<LocationJerkGraphFieldModel>();

            jerkGraphFields.Add(new LocationJerkGraphFieldModel()
            {
                DisplayName = "Forward Jerk (g/s)",
                Name = "ForwardJerk",
                Type = "double"
            });

            jerkGraphFields.Add(new LocationJerkGraphFieldModel()
            {
                DisplayName = "Lateral Jerk (g/s)",
                Name = "LateralJerk",
                Type = "double"
            });

            jerkGraphFields.Add(new LocationJerkGraphFieldModel()
            {
                DisplayName = "Vertical Jerk (g/s)",
                Name = "VerticalJerk",
                Type = "double"
            });

            return jerkGraphFields;
        }

        private IEnumerable<LocationJerkGraphModel> GetLocationJerkGraphModels(List<JerkModel> capturedJerks)
        {
            List<LocationJerkGraphModel> models = new List<LocationJerkGraphModel>();
            if (capturedJerks != null)
            {
                LocationJerkGraphModel model;
                foreach (JerkModel jerk in capturedJerks)
                {
                    model = new LocationJerkGraphModel();

                    model.Timestamp = jerk.JerkTimeStamp;
                    model.Values.Add("ForwardJerk", jerk.ForwardJerk);
                    model.Values.Add("LateralJerk", jerk.LateralJerk);
                    model.Values.Add("VerticalJerk", jerk.VerticalJerk);

                    models.Add(model);
                }
            }

            return models;
        }
    }
}