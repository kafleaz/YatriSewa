using System.Collections.Generic;
using System.Threading.Tasks;
using YatriSewa.Models;
using Route = YatriSewa.Models.Route;
namespace YatriSewa.Services.Interfaces
{
    
  
        public interface IOperatorService
        {
            Task<IEnumerable<Bus>> GetBusesByUserIdAsync(string userId);
            Task<Bus?> GetBusDetailsAsync(int id);
            Task AddBusAsync(Bus bus, string userId);
            Task<IEnumerable<Route>> GetRoutesByUserIdAsync(string userId);
            Task<Route?> GetRouteDetailsAsync(int id);
            Task AddRouteAsync(Route route, string userId);
        }
    
}
