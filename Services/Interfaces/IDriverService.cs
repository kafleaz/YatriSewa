using System.Collections.Generic;
using System.Threading.Tasks;
using YatriSewa.Models;

public interface IDriverService
{
    Task<IEnumerable<Schedule>> GetSchedulesForDriverAsync(int driverId, DateTime date);
    Task<IEnumerable<object>> GetPassengersByScheduleIdAsync(int scheduleId);
    Task<Schedule> GetScheduleDetailsAsync(int scheduleId);
    Task<IEnumerable<BusDriver>> GetAllDriversAsync();
    Task<IEnumerable<object>> GetJourneyListByDateAsync(DateTime date);

    Task<IEnumerable<object>> GetJourneyListAsync();
}


