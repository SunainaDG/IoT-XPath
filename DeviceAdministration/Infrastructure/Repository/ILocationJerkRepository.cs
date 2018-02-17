using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Repository
{
    public interface ILocationJerkRepository
    {
        Task<IEnumerable<LocationJerkModel>> LoadLatestLocationJerkInfoAsync();
        Task SaveLocationJerkInfoAsync(List<LocationJerkModel> newLocationJerks);
        Task<bool> DeleteLocationBlob();
        Task DeleteLocationJerkAsync(double? latitude, double? longitude);
        Task DeleteLocationsInBatchAsync(IEnumerable<LocationModel> locations);

    }
}
