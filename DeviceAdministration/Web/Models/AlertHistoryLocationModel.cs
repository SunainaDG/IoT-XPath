using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Web.Models
{
    public class AlertHistoryLocationModel
    {
        public double Latitude
        {
            get;
            set;
        }

        public double Longitude
        {
            get;
            set;
        }

        public LocationStatus Status
        {
            get;
            set;
        }

        public int TotalDeviceCount
        {
            get;
            set;
        }

        public double MaxJerk{ get; set; }

        public List<MajorLocationJerk> HighestJerks { get; set; }
    }

    public class MajorLocationJerk
    {
        public string DeviceId { get; set; }
        public double JerkValue { get; set; }
        public string JerkType { get; set; }
        public DateTime? JerkTime { get; set; }
    }
}