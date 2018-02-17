using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;
using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public class LocationJerkLogic : ILocationJerkLogic
    {
        private readonly ILocationJerkRepository _locationJerkRepository;

        public LocationJerkLogic(ILocationJerkRepository locationJerkRepository)
        {
            _locationJerkRepository = locationJerkRepository;
        }

        public async Task<IEnumerable<LocationJerkModel>> LoadLatestLocationJerkInfoAsync()
        {
            IEnumerable<LocationJerkModel> locationJerkList = await _locationJerkRepository.LoadLatestLocationJerkInfoAsync();

            return locationJerkList;
        }

        public async Task SaveLocationJerkInfoAsync(List<LocationJerkModel> newLocationJerks)
        {
            await _locationJerkRepository.SaveLocationJerkInfoAsync(newLocationJerks);
        }

        public async Task<LocationJerkListModel> GetLocationJerkListModel()
        {
            var result = new LocationJerkListModel();

            // Initialize defaults to opposite extremes to ensure mins and maxes are beyond any actual values
            double minLat = double.MaxValue;
            double maxLat = double.MinValue;
            double minLong = double.MaxValue;
            double maxLong = double.MinValue;

            var locationList = new List<LocationJerkModel>();
            IEnumerable<LocationJerkModel> fetchedData = await LoadLatestLocationJerkInfoAsync();

            if (fetchedData != null)
            {
                locationList.AddRange(fetchedData);
            }

            if (locationList != null && locationList.Count > 0)
            {
                foreach (LocationJerkModel location in locationList)
                {
                    if (location.DeviceList != null && location.Latitude != null && location.Longitude != null)
                    {
                        double latitude = (double)location.Latitude;
                        double longitude = (double)location.Longitude;

                        if (longitude < minLong)
                        {
                            minLong = longitude;
                        }
                        if (longitude > maxLong)
                        {
                            maxLong = longitude;
                        }
                        if (latitude < minLat)
                        {
                            minLat = latitude;
                        }
                        if (latitude > maxLat)
                        {
                            maxLat = latitude;
                        }


                    }
                }
            }

            if (locationList.Count == 0)
            {
                // reinitialize bounds to center on Seattle area if no devices
                minLat = 47.6;
                maxLat = 47.6;
                minLong = -122.3;
                maxLong = -122.3;
            }

            double offset = 0.05;

            result.LocationJerkList = locationList;
            result.MinimumLatitude = minLat - offset;
            result.MaximumLatitude = maxLat + offset;
            result.MinimumLongitude = minLong - offset;
            result.MaximumLongitude = maxLong + offset;

            return result;
        }

        public async Task<LocationJerkModelExtended> GetLocationDetails(double? latitude, double? longitude)
        {
            IEnumerable<LocationJerkModel> fetchedData = await _locationJerkRepository.LoadLatestLocationJerkInfoAsync();

            if (latitude == null || longitude == null)
            {
                return null;
            }

            if (fetchedData != null)
            {
                LocationJerkModel queriedData = fetchedData.FirstOrDefault(loc => loc.Latitude == latitude && loc.Longitude == longitude);

                if (queriedData != null)
                {
                    LocationJerkModelExtended location = new LocationJerkModelExtended
                    {
                        Latitude = queriedData.Latitude,
                        Longitude = queriedData.Longitude,
                        Status = queriedData.Status,
                        Altitude = queriedData.Altitude,
                        DeviceList = queriedData.DeviceList,
                        NoOfDevices = queriedData.DeviceList.Count
                    };

                    return location;
                }
            }

            return null;
        }

        public async Task<bool> DeleteAllLocationJerksAsync()
        {
            return await _locationJerkRepository.DeleteLocationBlob();
        }

        public async Task DeleteLocationJerkAsync(double? latitude, double? longitude)
        {
            await _locationJerkRepository.DeleteLocationJerkAsync(latitude, longitude);
        }

        public async Task DeleteLocationsInBatchAsync(IEnumerable<LocationModel> locations)
        {
            await _locationJerkRepository.DeleteLocationsInBatchAsync(locations);
        }
    }
}
