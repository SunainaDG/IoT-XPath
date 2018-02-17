using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using GlobalResources;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class EditLocationRuleModel
    {
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
        public bool IsCreateRequest { get; set; }
        public string CheckForErrorMessage()
        {
            if (string.IsNullOrWhiteSpace(RegionId) || string.IsNullOrWhiteSpace(RuleId))
            {
                return Strings.MandatoryFieldsMissing;
            }

            return null;
        }
    }
}