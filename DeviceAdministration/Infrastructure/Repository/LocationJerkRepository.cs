using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Configurations;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.Common.Helpers;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.WindowsAzure.Storage;
using Newtonsoft.Json;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    /// <summary>
    /// A repository for Location Jerk Information.
    /// </summary>
    public class LocationJerkRepository : ILocationJerkRepository
    {
        private readonly string _blobName;
        private readonly IBlobStorageClient _blobStorageManager;

        /// <summary>
        /// Initializes a new instance of the LocationJerkRepository class.
        /// </summary>
        /// <param name="configProvider">
        /// The IConfigurationProvider implementation with which to initialize 
        /// the new instance.
        /// </param>
        /// <param name="blobStorageClientFactory">
        /// The IBlobStorageClientFactory implementation with which to initialize 
        /// the new instance.
        /// </param>
        public LocationJerkRepository(IConfigurationProvider configProvider, IBlobStorageClientFactory blobStorageClientFactory)
        {
            if (configProvider == null)
            {
                throw new ArgumentNullException("configProvider");
            }

            string blobName = configProvider.GetConfigurationSettingValue("LocationJerkBlobName");
            string connectionString = configProvider.GetConfigurationSettingValue("device.StorageConnectionString");
            string containerName = configProvider.GetConfigurationSettingValue("LocationJerkContainerName");

            _blobName = blobName;
            _blobStorageManager = blobStorageClientFactory.CreateClient(connectionString, containerName);
        }

        public async Task<IEnumerable<LocationJerkModel>> LoadLatestLocationJerkInfoAsync()
        {
            var locationJerkBlobResults = await GetAllLocationJerkInfo();
            return locationJerkBlobResults.LocationJerkInfo;
        }

        public async Task SaveLocationJerkInfoAsync(List<LocationJerkModel> newLocationJerks)
        {
            LocationJerkBlobResults existingBlobResults = await GetAllLocationJerkInfo();

            List<LocationJerkModel> existingLocationJerks = existingBlobResults.LocationJerkInfo;

            foreach (LocationJerkModel newJerkModel in newLocationJerks)
            {
                if (newJerkModel.Latitude == null)
                {
                    throw new Exception("No Latitude Found in LocationJerkModel-- from LocationJerkRepository.SaveLocationJerkInfoAsync");
                }

                if (newJerkModel.Longitude == null)
                {
                    throw new Exception("No Longitude Found in LocationJerkModel-- from LocationJerkRepository.SaveLocationJerkInfoAsync");
                }

                if (newJerkModel.DeviceList == null)
                {
                    throw new Exception("No DeviceList Found in LocationJerkModel-- from LocationJerkRepository.SaveLocationJerkInfoAsync");
                }

                if (newJerkModel.Latitude != null && newJerkModel.Longitude != null)
                {
                    LocationJerkModel found = (existingLocationJerks as IEnumerable<LocationJerkModel>).FirstOrDefault(j => j.Latitude == newJerkModel.Latitude && j.Longitude == newJerkModel.Longitude);

                    if (found == null)
                    {
                        if (newJerkModel.DeviceList != null)
                        {
                            foreach (DeviceJerkModel jerkDevice in newJerkModel.DeviceList)
                            {
                                if (jerkDevice.DeviceId == String.Empty || String.IsNullOrWhiteSpace(jerkDevice.DeviceId))
                                {
                                    throw new Exception("No DeviceID Found in Jerked Device-- from LocationJerkRepository.SaveLocationJerkInfoAsync");
                                }

                                if (jerkDevice.CapturedJerks == null || jerkDevice.DeviceId == String.Empty || String.IsNullOrWhiteSpace(jerkDevice.DeviceId))
                                {
                                    newJerkModel.DeviceList.Remove(jerkDevice);
                                }
                            }

                            if (newJerkModel.DeviceList != null && newJerkModel.DeviceList.Count() > 0)
                            {
                                existingLocationJerks.Add(newJerkModel);
                            }                            
                        }
                    }
                    else
                    {
                        List<DeviceJerkModel> newDevices = newJerkModel.DeviceList;
                        foreach (DeviceJerkModel jerkDevice in newDevices)
                        {
                            if (jerkDevice.DeviceId == String.Empty || String.IsNullOrWhiteSpace(jerkDevice.DeviceId))
                            {
                                throw new Exception("No DeviceID Found in Jerked Device-- from LocationJerkRepository.SaveLocationJerkInfoAsync");
                            }

                            if (jerkDevice.DeviceId != String.Empty || !String.IsNullOrWhiteSpace(jerkDevice.DeviceId))
                            {
                                DeviceJerkModel deviceFound = (found.DeviceList as IEnumerable<DeviceJerkModel>).FirstOrDefault(d => d.DeviceId == jerkDevice.DeviceId);

                                if (jerkDevice.CapturedJerks != null)
                                {
                                    if (deviceFound == null)
                                    {
                                        found.DeviceList.Add(jerkDevice);
                                    }
                                    else
                                    {
                                        deviceFound.CapturedJerks.AddRange(jerkDevice.CapturedJerks);
                                    }
                                }                                
                            }                            
                        }
                    }

                    //add code here for setting status of Location
                }
            }

            string newJsonData = JsonConvert.SerializeObject(existingLocationJerks);
            byte[] newBytes = Encoding.UTF8.GetBytes(newJsonData);

            await _blobStorageManager.UploadFromByteArrayAsync(
                _blobName,
                newBytes,
                0,
                newBytes.Length,
                AccessCondition.GenerateIfMatchCondition(existingBlobResults.ETag),
                null,
                null);
        }

        private async Task<LocationJerkBlobResults> GetAllLocationJerkInfo()
        {
            var locationjerkinfo = new List<LocationJerkModel>();
            byte[] blobData = await _blobStorageManager.GetBlobData(_blobName);

            if (blobData != null && blobData.Length > 0)
            {
                //get existing location jerk info in object form
                string existingJsonData = Encoding.UTF8.GetString(blobData);
                
                try
                {
                    locationjerkinfo = JsonConvert.DeserializeObject<List<LocationJerkModel>>(existingJsonData);
                }
                catch (Exception ex)
                {
                    throw new ArgumentException("Invalid Json format");
                }

                string etag = await _blobStorageManager.GetBlobEtag(_blobName);
                return new LocationJerkBlobResults(locationjerkinfo, etag);
            }

            return new LocationJerkBlobResults(locationjerkinfo, "");
        }

        public async Task DeleteLocationJerkAsync(double? latitude, double? longitude)
        {
            LocationJerkBlobResults existingBlobResults = await GetAllLocationJerkInfo();
            List<LocationJerkModel> existingLocationJerks = existingBlobResults.LocationJerkInfo;


            if (latitude == null)
            {
                throw new Exception("No Latitude Found in LocationJerkModel-- from LocationJerkRepository.SaveLocationJerkInfoAsync");
            }

            if (longitude == null)
            {
                throw new Exception("No Longitude Found in LocationJerkModel-- from LocationJerkRepository.SaveLocationJerkInfoAsync");
            }

            if (latitude != null && longitude != null)
            {
                LocationJerkModel found = (existingLocationJerks as IEnumerable<LocationJerkModel>).FirstOrDefault(j => j.Latitude == latitude && j.Longitude == longitude);

                if (found != null)
                {
                    existingLocationJerks.Remove(found);
                }

            }

            string newJsonData = JsonConvert.SerializeObject(existingLocationJerks);
            byte[] newBytes = Encoding.UTF8.GetBytes(newJsonData);

            await _blobStorageManager.UploadFromByteArrayAsync(
                _blobName,
                newBytes,
                0,
                newBytes.Length,
                AccessCondition.GenerateIfMatchCondition(existingBlobResults.ETag),
                null,
                null);
        }

        public async Task DeleteLocationsInBatchAsync(IEnumerable<LocationModel> locations)
        {
            if (locations != null)
            {
                LocationJerkBlobResults existingBlobResults = await GetAllLocationJerkInfo();
                List<LocationJerkModel> existingLocationJerks = existingBlobResults.LocationJerkInfo;
                LocationJerkModel found = null;

                foreach (var location in locations)
                {
                    found = (existingLocationJerks as IEnumerable<LocationJerkModel>).FirstOrDefault(j => j.Latitude == location.Latitude && j.Longitude == location.Longitude);

                    if (found != null)
                    {
                        existingLocationJerks.Remove(found);
                    }

                    found = null;
                }

                string newJsonData = JsonConvert.SerializeObject(existingLocationJerks);
                byte[] newBytes = Encoding.UTF8.GetBytes(newJsonData);

                await _blobStorageManager.UploadFromByteArrayAsync(
                    _blobName,
                    newBytes,
                    0,
                    newBytes.Length,
                    AccessCondition.GenerateIfMatchCondition(existingBlobResults.ETag),
                    null,
                    null);
            }
        }

        public async Task<bool> DeleteLocationBlob()
        {
            //return await _blobStorageManager.DeleteBlob(_blobName);
            return await _blobStorageManager.DeleteBlob("testBlob.json");
        }

        private class LocationJerkBlobResults
        {
            public LocationJerkBlobResults(List<LocationJerkModel> locationJerkInfo, string eTag)
            {
                LocationJerkInfo = locationJerkInfo;
                ETag = eTag;
            }

            public List<LocationJerkModel> LocationJerkInfo { get; private set; }
            public string ETag { get; private set; }
        }
    }
}
