using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YatriSewa.Models;

public class DriverService : IDriverService
{
    private readonly ApplicationContext _context;

    public DriverService(ApplicationContext context)
    {
        _context = context;
    }

    // Fetch schedules assigned to a driver on a specific date
    public async Task<IEnumerable<Schedule>> GetSchedulesForDriverAsync(int driverId, DateTime date)
    {
        // Start and end of the selected date
        var startOfDay = date.Date;             // e.g., 2025-01-21 00:00:00
        var endOfDay = date.Date.AddDays(1);    // e.g., 2025-01-22 00:00:00

        // Query to fetch schedules
        return await _context.Schedule_Table
            .Where(s => s.DriverId == driverId &&
                        s.DepartureTime >= startOfDay &&
                        s.DepartureTime < endOfDay) // Matches within the date
            .Include(s => s.Bus)  // Include related Bus details
            .Include(s => s.Route)  // Include related Route details
            .ToListAsync();
    }


    // Fetch passengers for a specific schedule
    public async Task<IEnumerable<Passenger>> GetPassengersByScheduleIdAsync(int scheduleId)
    {
        var schedule = await _context.Schedule_Table
            .Include(s => s.Bus)
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);

        if (schedule == null || schedule.BusId == null)
        {
            return new List<Passenger>();
        }

        return await _context.Passenger_Table
            .Where(p => p.BusId == schedule.BusId)
            .ToListAsync();
    }

    // Fetch detailed schedule information
    public async Task<Schedule> GetScheduleDetailsAsync(int scheduleId)
    {
        return await _context.Schedule_Table
            .Include(s => s.Bus) // Include Bus details
            .Include(s => s.Route) // Include Route details
            .Include(s => s.Driver) // Include Driver details
            .FirstOrDefaultAsync(s => s.ScheduleId == scheduleId);
    }
    //fetch all bus drivers
    public async Task<IEnumerable<BusDriver>> GetAllDriversAsync()
    {
        // Fetch all drivers from the database
        return await _context.Driver_Table.ToListAsync();
    }
    public async Task<IEnumerable<object>> GetJourneyListAsync()
    {
        return await _context.Schedule_Table
            .Include(s => s.Route)
               .Include(s => s.Bus) // Include the Route table
            .Select(schedule => new
            {
                StartLocation = schedule.Route.StartLocation, // Assuming Route has this property
                StopLocations = schedule.Route.Stops,        // Assuming Stops is a string or list
                EndLocation = schedule.Route.EndLocation,    // Assuming Route has this property
                BusId = schedule.BusId,
                BusName = schedule.Bus.BusName,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime
            })
            .ToListAsync();
    }



}
