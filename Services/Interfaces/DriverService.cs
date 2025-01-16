using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YatriSewa.Models;
using YatriSewa.Services.Interfaces;
namespace YatriSewa.Services
{
    public class DriverService : IDriverService
    {
        private readonly ApplicationContext _context;

        public DriverService(ApplicationContext context)
        {
            _context = context;
        }

        public BusDetails GetBusDetails()
        {
            // Retrieve bus details dynamically from the database if necessary
            // Assuming the bus details are managed elsewhere
            throw new NotImplementedException();
        }

        public List<PassengerListForDay> GetPassengerListForTwoDays()
        {
            // Fetch passengers with RoleId == 1 (passengers)
            var passengers = _context.Users
                .Where(u => u.RoleId == 1)
                .Select(u => new Passenger
                {
                    Name = u.Name,
                    Age = u.Age,
                    SeatNumber = u.SeatNumber,
                    TripDate = u.TripDate
                })
                .ToList();

            // Group passengers by today and tomorrow
            var groupedPassengers = new List<PassengerListForDay>
            {
                new PassengerListForDay
                {
                    Date = DateTime.Today,
                    Passengers = passengers.Where(p => p.TripDate.Date == DateTime.Today).ToList()
                },
                new PassengerListForDay
                {
                    Date = DateTime.Today.AddDays(1),
                    Passengers = passengers.Where(p => p.TripDate.Date == DateTime.Today.AddDays(1)).ToList()
                }
            };

            return groupedPassengers;
        }

        public List<Route> GetRoutes()
        {
            // Retrieve routes dynamically from the database if necessary
            throw new NotImplementedException();
        }

        public Location GetCurrentLocation()
        {
            // Fetch location data dynamically (e.g., from a tracking service)
            throw new NotImplementedException();
        }
    }
}
