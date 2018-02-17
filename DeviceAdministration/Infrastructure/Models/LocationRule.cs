using System;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class LocationRule
    {
        public LocationRule() { }

        public LocationRule(string ruleId)
        {
            RuleId = ruleId;
        }
        
        public bool EnabledState { get; set; }
        public string RegionId { get; set; }
        public string RuleId { get; set; }

        public string Region { get; set; }
        public double RegionLatitude { get; set; }
        public double RegionLongitude { get; set; }
        public double VerticalThreshold { get; set; }
        public double LateralThreshold { get; set; }
        public double ForwardThreshold { get; set; }
        public string RuleOutput { get; set; }
        public string Etag { get; set; }

        /// <summary>
        /// This method will initialize any required, and automatically-built properties for a new rule
        /// </summary>
        public void InitializeNewRule(string regionId, double latitude, double longitude)
        {
            RegionId = regionId;
            RegionLatitude = latitude;
            RegionLongitude = longitude;
            EnabledState = true;
        }
    }
}
