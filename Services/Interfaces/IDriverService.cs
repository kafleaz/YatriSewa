using System.Collections.Generic;
using YatriSewa.Models;

namespace YatriSewa.Services.Interfaces
{
    public interface IDriverService
    {
        BusDetails GetBusDetails();
        List<PassengerListForDay> GetPassengerListForTwoDays();
        List<Route> GetRoutes();
        Location GetCurrentLocation();
    }
}
