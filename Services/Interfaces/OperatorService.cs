// Services/OperatorService.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YatriSewa.Models;
using YatriSewa.Services.Interfaces;
using Route = YatriSewa.Models.Route;

namespace YatriSewa.Services
{
    public class OperatorService : IOperatorService
    {
        private readonly ApplicationContext _context; // Database context

        public OperatorService(ApplicationContext context)
        {
            _context = context; // Initialize context
        }

        public async Task<IEnumerable<User>> GetAllOperatorsAsync()
        {
            return await _context.User_Table
                .Where(u => u.Role == UserRole.Operator) // Assuming you have a Role property to distinguish users
                .Include(u => u.BusCompany) // Include related BusCompany if needed
                .ToListAsync();
        }


        public async Task<IEnumerable<BusCompany>> GetAllOperatorAsync()
        {
            return await _context.Company_Table.ToListAsync(); // Fetch BusCompany data
        }



        public async Task<User?> GetOperatorByIdAsync(int id)
        {
            return await _context.User_Table.FindAsync(id); // Fetch operator by ID
        }

        public async Task<IEnumerable<Bus>> GetBusesByUserIdAsync(string userId)
        {
            var user = await _context.User_Table.Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user?.BusCompany == null)
            {
                return Enumerable.Empty<Bus>();
            }

            return await _context.Bus_Table
                .Where(b => b.CompanyId == user.BusCompany.CompanyId)
                .Include(b => b.BusCompany)
                .Include(b => b.BusDriver)
                .Include(b => b.Route)
                .ToListAsync();
        }

        public async Task<Bus?> GetBusDetailsAsync(int id)
        {
            return await _context.Bus_Table
                .Include(b => b.BusCompany)
                .Include(b => b.BusDriver)
                .Include(b => b.Route)
                .FirstOrDefaultAsync(b => b.BusId == id);
        }

        public async Task AddBusAsync(Bus bus, string userId)
        {
            var currentUser = await _context.User_Table.Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (currentUser?.BusCompany == null)
            {
                throw new Exception("User is not associated with a company.");
            }

            bus.CompanyId = currentUser.BusCompany.CompanyId;

            if (!await _context.Route_Table.AnyAsync(r => r.RouteID == bus.RouteId))
            {
                throw new Exception($"Invalid RouteID: {bus.RouteId}");
            }

            if (!await _context.Driver_Table.AnyAsync(d => d.DriverId == bus.DriverId))
            {
                throw new Exception($"Invalid DriverID: {bus.DriverId}");
            }

            _context.Add(bus);
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<Route>> GetRoutesByUserIdAsync(string userId)
        {
            var user = await _context.User_Table.Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user?.BusCompany == null)
            {
                return Enumerable.Empty<YatriSewa.Models.Route>();
            }

            return await _context.Route_Table
                .Where(r => r.CompanyID == user.BusCompany.CompanyId)
                .ToListAsync();
        }

        public async Task<User?> GetCurrentUserWithCompanyAsync(string userId)
        {
            if (!int.TryParse(userId, out int parsedUserId))
            {
                return null;
            }

            return await _context.User_Table.Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId == parsedUserId);
        }
        public async Task<List<Bus>> GetBusesByCompanyIdAsync(int companyId)
        {
            return await _context.Bus_Table
                .Where(bus => bus.CompanyId == companyId)
                .Include(bus => bus.Route) // Optional: Include related data
                .Include(bus => bus.BusDriver) // Optional: Include related data
                .ToListAsync();
        }

        public async Task<IEnumerable<Route>> GetRoutesByCompanyIdAsync(int companyId)
        {
                return await _context.Route_Table
                    .Where(r => r.CompanyID == companyId)
                    .ToListAsync();

        }

        public async Task<IEnumerable<BusDriver>> GetDriversByCompanyIdAsync(int companyId)
        {
            return await _context.Driver_Table.Where(d => d.CompanyId == companyId).ToListAsync();
        }



        public async Task<Route?> GetRouteDetailsAsync(int id)
        {
            return await _context.Route_Table.FindAsync(id);
        }

        public async Task AddRouteAsync(YatriSewa.Models.Route route, string userId)
        {
            var currentUser = await _context.User_Table.Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (currentUser?.BusCompany == null)
            {
                throw new Exception("User is not associated with a company.");
            }
            
                route.CompanyID = currentUser.BusCompany.CompanyId;
                _context.Add(route);
                await _context.SaveChangesAsync();
            
        }

        public Task<(bool success, string message)> AddBusDetailsAsync(Bus bus, string userId)
        {
            throw new NotImplementedException();
        }

        public async Task<BusCompany?> GetCompanyByIdAsync(int companyId)
        {
            return await _context.Company_Table.FirstOrDefaultAsync(c => c.CompanyId == companyId);
        }
      


    }
}