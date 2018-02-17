using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage.Table;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// Class for working with persistence of Location Rules data.
    /// Note that we store device rules in an Azure table, but we also need
    /// to save a different format as a blob for the rules ASA job to pickup.
    /// The ASA rules job uses that blob as ref data and joins the most 
    /// recent version to the incoming data stream from the IoT Hub.
    /// (The ASA job checks for new rules blobs every minute at a well-known path)
    /// </summary>
    public class LocationRulesRepository : ILocationRulesRepository
    {
        private readonly string _blobName;
        private readonly string _storageAccountConnectionString;
        private readonly string _locationRulesBlobStoreContainerName;
        private readonly string _locationRulesNormalizedTableName;
        private readonly IAzureTableStorageClient _azureTableStorageClient;
        private readonly IBlobStorageClient _blobStorageClient;

        private DateTimeFormatInfo _formatInfo;

        public LocationRulesRepository(IConfigurationProvider configurationProvider, IAzureTableStorageClientFactory tableStorageClientFactory, IBlobStorageClientFactory blobStorageClientFactory)
        {
            _storageAccountConnectionString = configurationProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            _locationRulesBlobStoreContainerName = configurationProvider.GetConfigurationSettingValue("LocationRulesStoreContainerName");
            _locationRulesNormalizedTableName = configurationProvider.GetConfigurationSettingValue("LocationRulesTableName");
            _azureTableStorageClient = tableStorageClientFactory.CreateClient(_storageAccountConnectionString, _locationRulesNormalizedTableName);
            _blobName = configurationProvider.GetConfigurationSettingValue("AsaRefLocationRulesBlobName");
            _blobStorageClient = blobStorageClientFactory.CreateClient(_storageAccountConnectionString, _locationRulesBlobStoreContainerName);

            // note: InvariantCulture is read-only, so use en-US and hardcode all relevant aspects
            CultureInfo culture = CultureInfo.CreateSpecificCulture("en-US");
            _formatInfo = culture.DateTimeFormat;
            _formatInfo.ShortDatePattern = @"yyyy-MM-dd";
            _formatInfo.ShortTimePattern = @"HH-mm";
        }

        /// <summary>
        /// Get all Location Rules from AzureTableStorage. If none are found it will return an empty list.
        /// </summary>
        /// <returns>All LocationRules or an empty list</returns>
        public async Task<List<LocationRule>> GetAllRulesAsync()
        {
            List<LocationRule> result = new List<LocationRule>();

            IEnumerable<LocationRuleTableEntity> queryResults = await GetAllRulesFromTable();
            foreach (LocationRuleTableEntity rule in queryResults)
            {
                var locationRule = BuildRuleFromTableEntity(rule);
                result.Add(locationRule);
            }

            return result;
        }

        /// <summary>
        /// Retrieve a single rule from AzureTableStorage or default if none exists. 
        /// A distinct rule is defined by the combination key regionID/ruleId
        /// </summary>
        /// <param name="regionLatitude"></param>
        /// <param name="regionLongitude"></param>
        /// <param name="ruleId"></param>
        /// <returns></returns>
        public async Task<LocationRule> GetLocationRuleAsync(string regionId, string ruleId)
        {
            TableOperation query = TableOperation.Retrieve<LocationRuleTableEntity>(regionId, ruleId);

            TableResult response = await Task.Run(() =>
                _azureTableStorageClient.Execute(query)
            );

            LocationRule result = BuildRuleFromTableEntity((LocationRuleTableEntity)response.Result);
            return result;
        }

        /// <summary>
        /// Retrieve all rules from the database that have been defined for a single location.
        /// If none exist an empty list will be returned. This method guarantees a non-null
        /// result.
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        public async Task<List<LocationRule>> GetAllRulesForRegionAsync(string regionId)
        {
            TableQuery<LocationRuleTableEntity> query = new TableQuery<LocationRuleTableEntity>().
                Where(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, regionId));
            var regionsResult = await _azureTableStorageClient.ExecuteQueryAsync(query);
            List<LocationRule> result = new List<LocationRule>();
            foreach (LocationRuleTableEntity entity in regionsResult)
            {
                result.Add(BuildRuleFromTableEntity(entity));
            }
            return result;
        }

        /// <summary>
        /// Save a Location Rule to the server. This may be either a new rule or an update to an existing rule. 
        /// </summary>
        /// <param name="updateContainer"></param>
        /// <returns></returns>
        public async Task<TableStorageResponse<LocationRule>> SaveLocationRuleAsync(LocationRule updatedRule)
        {
            LocationRuleTableEntity incomingEntity = BuildTableEntityFromRule(updatedRule);

            TableStorageResponse<LocationRule> result =
                await _azureTableStorageClient.DoTableInsertOrReplaceAsync<LocationRule, LocationRuleTableEntity>(incomingEntity, BuildRuleFromTableEntity);

            if (result.Status == TableStorageResponseStatus.Successful)
            {
                // Build up a new blob to push up for ASA job ref data
                List<LocationRuleBlobEntity> blobList = await BuildBlobEntityListFromTableRows();
                await PersistRulesToBlobStorageAsync(blobList);
            }

            return result;
        }

        public async Task<TableStorageResponse<LocationRule>> DeleteLocationRuleAsync(LocationRule ruleToDelete)
        {
            LocationRuleTableEntity incomingEntity = BuildTableEntityFromRule(ruleToDelete);

            TableStorageResponse<LocationRule> result =
                await _azureTableStorageClient.DoDeleteAsync<LocationRule, LocationRuleTableEntity>(incomingEntity, BuildRuleFromTableEntity);

            if (result.Status == TableStorageResponseStatus.Successful)
            {
                // Build up a new blob to push up for ASA job ref data
                List<LocationRuleBlobEntity> blobList = await BuildBlobEntityListFromTableRows();
                await PersistRulesToBlobStorageAsync(blobList);
            }

            return result;
        }        

        private async Task<IEnumerable<LocationRuleTableEntity>> GetAllRulesFromTable()
        {
            TableQuery<LocationRuleTableEntity> query = new TableQuery<LocationRuleTableEntity>();

            return await _azureTableStorageClient.ExecuteQueryAsync(query);
        }

        private LocationRuleTableEntity BuildTableEntityFromRule(LocationRule incomingRule)
        {
            LocationRuleTableEntity tableEntity =
                new LocationRuleTableEntity(incomingRule.RegionId, incomingRule.RuleId)
                {
                    Enabled = incomingRule.EnabledState,
                    Region = incomingRule.Region,
                    RegionLatitude = incomingRule.RegionLatitude,
                    RegionLongitude = incomingRule.RegionLongitude,
                    VerticalThreshold = incomingRule.VerticalThreshold,
                    LateralThreshold = incomingRule.LateralThreshold,
                    ForwardThreshold = incomingRule.ForwardThreshold,
                    RuleOutput = incomingRule.RuleOutput
                };

            if (!string.IsNullOrWhiteSpace(incomingRule.Etag))
            {
                tableEntity.ETag = incomingRule.Etag;
            }

            return tableEntity;
        }

        private LocationRule BuildRuleFromTableEntity(LocationRuleTableEntity tableEntity)
        {
            if (tableEntity == null)
            {
                return null;
            }

            var updatedRule = new LocationRule(tableEntity.RuleId)
            {
                RegionId = tableEntity.RegionId,
                Region = tableEntity.Region,
                EnabledState = tableEntity.Enabled,
                RegionLatitude = tableEntity.RegionLatitude,
                RegionLongitude = tableEntity.RegionLongitude,
                VerticalThreshold = tableEntity.VerticalThreshold,
                LateralThreshold = tableEntity.LateralThreshold,
                ForwardThreshold = tableEntity.ForwardThreshold,                
                RuleOutput = tableEntity.RuleOutput,
                Etag = tableEntity.ETag
            };

            return updatedRule;
        }

        /// <summary>
        /// Compile all rows from the table storage into the format used in the blob storage for
        /// ASA job reference data.
        /// </summary>
        /// <returns></returns>
        private async Task<List<LocationRuleBlobEntity>> BuildBlobEntityListFromTableRows()
        {
            IEnumerable<LocationRuleTableEntity> queryResults = await GetAllRulesFromTable();
            Dictionary<string, LocationRuleBlobEntity> blobEntityDictionary = new Dictionary<string, LocationRuleBlobEntity>();
            foreach (LocationRuleTableEntity rule in queryResults)
            {
                if (rule.Enabled)
                {
                    LocationRuleBlobEntity entity = null;
                    string regionRuleId = $"{rule.PartitionKey}_{rule.RowKey}";
                    //if (!blobEntityDictionary.ContainsKey(locationRuleId))
                    //{
                    //    entity = new LocationRuleBlobEntity();
                    //    blobEntityDictionary.Add(locationRuleId, entity);
                    //}
                    //else
                    //{
                    //    entity = blobEntityDictionary[rule.PartitionKey];
                    //}

                    if (!blobEntityDictionary.ContainsKey(regionRuleId))
                    {
                        entity = new LocationRuleBlobEntity()
                        {
                            Id = $"{rule.RegionLatitude}_{rule.RegionLongitude}",
                            RegionLatitude = rule.RegionLatitude,
                            RegionLongitude = rule.RegionLongitude,
                            Vertical = rule.VerticalThreshold,
                            Lateral = rule.LateralThreshold,
                            Forward = rule.ForwardThreshold,
                            RuleOutput = rule.RuleOutput
                        };
                        blobEntityDictionary.Add(regionRuleId, entity);
                    }
                }
            }

            return blobEntityDictionary.Values.ToList();
        }

        //When we save data to the blob storage for use as ref data on an ASA job, ASA picks that
        //data up based on the current time, and the data must be finished uploading before that time.
        //
        //From the Azure Team: "What this means is your blob in the path 
        //<...>/devicerules/2015-09-23/15-24/devicerules.json needs to be uploaded before the clock 
        //strikes 2015-09-23 15:25:00 UTC, preferably before 2015-09-23 15:24:00 UTC to be used when 
        //the clock strikes 2015-09-23 15:24:00 UTC"
        //
        //If we have many devices, an upload could take a measurable amount of time.
        //
        //Also, it is possible that the ASA clock is not precisely in sync with the
        //server clock. We want to store our update on a path slightly ahead of the current time so
        //that by the time ASA reads it we will no longer be making any updates to that blob -- i.e.
        //all current changes go into a future blob. We will choose two minutes into the future. In the
        //worst case, if we make a change at 12:03:59 and our write is delayed by ten seconds (until 
        //12:04:09) it will still be saved on the path {date}\12-05 and will be waiting for ASA to 
        //find in one minute.
        private const int blobSaveMinutesInTheFuture = 2;
        private async Task PersistRulesToBlobStorageAsync(List<LocationRuleBlobEntity> blobList)
        {
            string updatedJson = JsonConvert.SerializeObject(blobList);
            //DateTime saveDate = DateTime.UtcNow.AddMinutes(blobSaveMinutesInTheFuture);
            //string dateString = saveDate.ToString("d", _formatInfo);
            //string timeString = saveDate.ToString("t", _formatInfo);
            //string blobName = string.Format(@"{0}\{1}\{2}", dateString, timeString, _blobName);

            //await _blobStorageClient.UploadTextAsync(blobName, updatedJson);
            await _blobStorageClient.UploadTextAsync(_blobName, updatedJson);
        }
    }
}
