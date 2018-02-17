using Microsoft.WindowsAzure.Storage.Table;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models
{
    public class LocationRuleTableEntity : TableEntity
    {
        public LocationRuleTableEntity(string regionId, string ruleId)
        {
            this.PartitionKey = regionId;
            this.RowKey = ruleId;
        }

        public LocationRuleTableEntity() { }

        [IgnoreProperty]
        public string RegionId
        {
            get { return this.PartitionKey; }
            set { this.PartitionKey = value; }
        }

        [IgnoreProperty]
        public string RuleId
        {
            get { return this.RowKey; }
            set { this.RowKey = value; }
        }

        public bool Enabled { get; set; }
        public string Region { get; set; }
        public double RegionLatitude { get; set; }
        public double RegionLongitude { get; set; }
        public double VerticalThreshold { get; set; }
        public double LateralThreshold { get; set; }
        public double ForwardThreshold { get; set; }
        public string RuleOutput { get; set; }
    }
}
