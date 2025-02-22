using Firebase.Database;
using Firebase.Database.Query;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using YatriSewa.Models;

public class FirebaseToDatabaseService
{
    private readonly ApplicationContext _context;

    public FirebaseToDatabaseService(ApplicationContext context)
    {
        _context = context;
    }

    public async Task FetchAndUpdateBusLocations()
    {
        // Initialize Firebase connection
        var firebase = new FirebaseClient("https://yatrisewa-4f81e-default-rtdb.asia-southeast1.firebasedatabase.app.yatrisewa-4f81e.firebaseapp.com");

        // Get data from Firebase
        var buses = await firebase
            .Child("bus") // Assuming Firebase stores buses under "bus"
            .OnceAsync<dynamic>();

        foreach (var bus in buses)
        {
            int busId = int.Parse(bus.Key); // Bus ID is assumed to be the key
            decimal latitude = (decimal)bus.Object.latitude;
            decimal longitude = (decimal)bus.Object.longitude;
            decimal speed = (decimal)bus.Object.speed;
            bool isMoving = (bool)bus.Object.isMoving;

            // Update or insert bus location in the database
            var existingLocation = await _context.IoTDeviceLocationLogs
                .FirstOrDefaultAsync(l => l.BusId == busId);

            if (existingLocation == null)
            {
                // Insert new location
                _context.IoTDeviceLocationLogs.Add(new IoTDeviceLocationLog
                {
                    BusId = busId,
                    Latitude = latitude,
                    Longitude = longitude,
                    Speed = speed,
                    Timestamp = DateTime.UtcNow,
                    IsMoving = isMoving
                });
            }
            else
            {
                // Update existing location
                existingLocation.Latitude = latitude;
                existingLocation.Longitude = longitude;
                existingLocation.Speed = speed;
                existingLocation.Timestamp = DateTime.UtcNow;
                existingLocation.IsMoving = isMoving;
            }
        }

        await _context.SaveChangesAsync(); // Save changes to the database
    }
}
