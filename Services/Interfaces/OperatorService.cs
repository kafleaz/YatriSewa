﻿// Services/Implementations/OperatorService.cs
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YatriSewa.Models;
using YatriSewa.Services.Interfaces;
using Route = YatriSewa.Models.Route;

namespace YatriSewa.Services.Implementations
{
    public class OperatorService : IOperatorService
    {
        private readonly ApplicationContext _context;

        public OperatorService(ApplicationContext context)
        {
            _context = context;
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
                .FirstOrDefaultAsync(m => m.BusId == id);
        }

        public async Task AddBusAsync(Bus bus, string userId)
        {
            var currentUser = await _context.User_Table.Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (currentUser?.BusCompany != null)
            {
                bus.CompanyId = currentUser.BusCompany.CompanyId;
                _context.Add(bus);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Route>> GetRoutesByUserIdAsync(string userId)
        {
            var user = await _context.User_Table.Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (user?.BusCompany == null)
            {
                return Enumerable.Empty<Route>();
            }

            return await _context.Route_Table
                .Where(r => r.CompanyID == user.BusCompany.CompanyId)
                .ToListAsync();
        }

        public async Task<Route?> GetRouteDetailsAsync(int id)
        {
            return await _context.Route_Table.FindAsync(id);
        }
        

        public async Task AddRouteAsync(Route route, string userId)
        {
            var currentUser = await _context.User_Table.Include(u => u.BusCompany)
                .FirstOrDefaultAsync(u => u.UserId.ToString() == userId);

            if (currentUser?.BusCompany != null)
            {
                route.CompanyID = currentUser.BusCompany.CompanyId;
                _context.Add(route);
                await _context.SaveChangesAsync();
            }
        }

        Task<IEnumerable<Route>> IOperatorService.GetRoutesByUserIdAsync(string userId)
        {
            throw new NotImplementedException();
        }

        Task<Route?> IOperatorService.GetRouteDetailsAsync(int id)
        {
            throw new NotImplementedException();
        }

       
    }
}