using System.Threading.Tasks;
using System.Collections.Generic;
using YatriSewa.Models;
using Route = YatriSewa.Models.Route;
namespace YatriSewa.Services.Interfaces
{
        public interface IOperatorService
        {
        Task<IEnumerable<User>> GetAllOperatorsAsync();
        Task<IEnumerable<BusCompany>> GetAllOperatorAsync();
        Task<IEnumerable<Bus>> GetBusesByUserIdAsync(string userId);
        Task<Bus?> GetBusDetailsAsync(int id);
        Task AddBusAsync(Bus bus, string userId);
        Task<IEnumerable<Route>> GetRoutesByUserIdAsync(string userId);
        Task<IEnumerable<Route>> GetRoutesByCompanyIdAsync(int companyId);
        Task<Route?> GetRouteDetailsAsync(int id);
        Task AddRouteAsync(Route route, string userId);
        Task<User?> GetCurrentUserWithCompanyAsync(string userId);
        Task<IEnumerable<BusDriver>> GetDriversByCompanyIdAsync(int companyId);
        Task<(bool success, string message)> AddBusDetailsAsync(Bus bus, string userId);
        Task<List<Bus>> GetBusesByCompanyIdAsync(int companyId);
        Task<BusCompany?> GetCompanyByIdAsync(int companyId);

        Task<IEnumerable<dynamic>> GetPassengersByScheduleIdAsync(int scheduleId);

    }

}
