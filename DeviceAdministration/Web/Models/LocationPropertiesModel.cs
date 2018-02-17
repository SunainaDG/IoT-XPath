using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using GlobalResources;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class LocationPropertiesModel
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? Altitude { get; set; }
        public string Status { get; set; }
        public List<DeviceJerkModel> DeviceList { get; set; }
        public string JsonList { get; set; }
        public int NoOfDevices { get; set; }

        public List<SelectListItem> JerkedDeviceSelectList { get; set; }
        public string RuleId { get; set; }
        public double RegionLatitude { get; set; }
        public double RegionLongitude { get; set; }

        public bool FromMap { get; set; }

        public string CheckForErrorMessage()
        {
            if(Latitude == null || Longitude == null)
            {
                return Strings.CoordinateFormatError;
            }

            return null;
        }
    }
}