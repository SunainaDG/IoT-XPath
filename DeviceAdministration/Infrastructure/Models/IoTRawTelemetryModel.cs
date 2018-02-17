using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class IoTRawTelemetryModel
    {
        private const string DEVICE_ID_COLUMN_NAME = "deviceid";
        private const string LATITUDE_COLUMN_NAME = "latitude";
        private const string LONGITUDE_COLUMN_NAME = "longitude";
        private const string ALTITUDE_COLUMN_NAME = "altitude";
        private const string HEADING_COLUMN_NAME = "heading";
        private const string SPEED_COLUMN_NAME = "speed";
        private const string MAPISBOUND_COLUMN_NAME = "mapisbound";
        private const string CAPTUREDJERKS_COLUMN_NAME = "capturedjerks";


        [JsonProperty(PropertyName = DEVICE_ID_COLUMN_NAME)]
        public string DeviceId { get; set; }

        [JsonProperty(PropertyName = LATITUDE_COLUMN_NAME)]
        public double Latitude { get; set; }

        [JsonProperty(PropertyName = LONGITUDE_COLUMN_NAME)]
        public double Longitude { get; set; }

        [JsonProperty(PropertyName = ALTITUDE_COLUMN_NAME)]
        public double Altitude { get; set; }

        [JsonProperty(PropertyName = HEADING_COLUMN_NAME)]
        public double Heading { get; set; }

        [JsonProperty(PropertyName = SPEED_COLUMN_NAME)]
        public double Speed { get; set; }

        [JsonProperty(PropertyName = MAPISBOUND_COLUMN_NAME)]
        public bool MapIsBound { get; set; }

        [JsonProperty(PropertyName = CAPTUREDJERKS_COLUMN_NAME)]
        public List<JerkModel> CapturedJerks { get; set; }
    }
}
