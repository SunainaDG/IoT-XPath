﻿using System;
using System.Runtime.Serialization;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Exceptions
{
    [Serializable]
    public class FilterDuplicatedNameException : Exception
    {
        public string FilterId { get; internal set; }
        public string FilterName { get; internal set; }

        public FilterDuplicatedNameException(string filterId, string filterName)
            : base(FormattableString.Invariant($"Failed to save Filter with ID = '{filterId}' and Name = '{filterName}', the filter name must be unique"))
        {
            FilterId = filterId;
            FilterName = filterName;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
