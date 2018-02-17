using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.Models;

namespace Microsoft.Azure.Devices.Applications.RemoteMonitoring.DeviceAdmin.Infrastructure.BusinessLogic
{
    public interface ILocationJerkLogic
    {
        Task<IEnumerable<LocationJerkModel>> LoadLatestLocationJerkInfoAsync();
        Task SaveLocationJerkInfoAsync(List<LocationJerkModel> newLocationJerks);
        Task<LocationJerkListModel> GetLocationJerkListModel();
        Task<LocationJerkModelExtended> GetLocationDetails(double? latitude, double? longitude);
        Task<bool> DeleteAllLocationJerksAsync();
        Task DeleteLocationJerkAsync(double? latitude, double? longitude);
        Task DeleteLocationsInBatchAsync(IEnumerable<LocationModel> locations);
    }
}
