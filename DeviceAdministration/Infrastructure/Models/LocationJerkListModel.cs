﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models
{
    public class LocationJerkListModel
    {
        public List<LocationJerkModel> LocationJerkList { get; set; }
        public double MinimumLatitude { get; set; }
        public double MaximumLatitude { get; set; }
        public double MinimumLongitude { get; set; }
        public double MaximumLongitude { get; set; }
    }
}
