//using Firebase.Database;
//using Firebase.Database.Query;
//using Microsoft.AspNetCore.SignalR;
//using Microsoft.EntityFrameworkCore;
//using System;
//using System.Threading;
//using System.Threading.Tasks;
//using YatriSewa.Models;

//public class RealTimeBusLocationService : BackgroundService
//{
//    private readonly IServiceProvider _serviceProvider; // Use IServiceProvider instead of directly injecting ApplicationContext
//    private readonly IHubContext<BusLocationHub> _hubContext;
//    private readonly FirebaseClient _firebaseClient;

//    public RealTimeBusLocationService(IServiceProvider serviceProvider, IHubContext<BusLocationHub> hubContext)
//    {
//        _serviceProvider = serviceProvider;
//        _hubContext = hubContext;
//        _firebaseClient = new FirebaseClient("https://yatrisewa-4f81e-default-rtdb.asia-southeast1.firebasedatabase.app/");
//    }

//    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
//    {
//        while (!stoppingToken.IsCancellationRequested)
//        {
//            try
//            {
//                using (var scope = _serviceProvider.CreateScope()) // Create a new scope
//                {
//                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

//                    var devices = await _firebaseClient
//                        .Child("devices")
//                        .OnceAsync<dynamic>();

//                    foreach (var device in devices)
//                    {
//                        string deviceIdentifier = device.Key;
//                        decimal latitude = (decimal)device.Object.latitude;
//                        decimal longitude = (decimal)device.Object.longitude;
//                        decimal speed = (decimal)device.Object.speed;

//                        Console.WriteLine($"Device: {deviceIdentifier}, Latitude: {latitude}, Longitude: {longitude}, Speed: {speed}");

//                        var existingDevice = await context.IoTDevices
//                            .Include(d => d.Bus)  // Include the related Bus entity
//                            .FirstOrDefaultAsync(d => d.DeviceIdentifier == deviceIdentifier);

//                        if (existingDevice == null)
//                        {
//                            var newDevice = new IoTDevice
//                            {
//                                DeviceIdentifier = deviceIdentifier,
//                                DeviceName = "New IoT Device", // Or assign dynamically
//                                Latitude = latitude,
//                                Longitude = longitude,
//                                Speed = speed,
//                                LastUpdated = DateTime.UtcNow
//                            };
//                            context.IoTDevices.Add(newDevice);
//                        }
//                        else
//                        {
//                            existingDevice.Latitude = latitude;
//                            existingDevice.Longitude = longitude;
//                            existingDevice.Speed = speed;
//                            existingDevice.LastUpdated = DateTime.UtcNow;

//                            context.IoTDevices.Update(existingDevice);
//                        }

//                        await context.SaveChangesAsync();

//                        // Use BusName if available, otherwise send "Unknown Bus"
//                        string busName = existingDevice?.Bus?.BusName ?? "Unknown Bus";
//                        await _hubContext.Clients.All.SendAsync("UpdateBusLocation", deviceIdentifier, busName, latitude, longitude, speed);
//                    }
//                }

//                await Task.Delay(10000, stoppingToken);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine($"Error in RealTimeBusLocationService: {ex.Message}");
//            }
//        }
//    }
//}

using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using YatriSewa.Models;
using YatriSewa.Services; // Import KalmanFilter

public class RealTimeBusLocationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<BusLocationHub> _hubContext;
    private readonly FirebaseClient _firebaseClient;

    // Store Kalman Filters for each bus
    private readonly Dictionary<string, KalmanFilter> latFilters = new();
    private readonly Dictionary<string, KalmanFilter> lonFilters = new();

    public RealTimeBusLocationService(IServiceProvider serviceProvider, IHubContext<BusLocationHub> hubContext)
    {
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
        _firebaseClient = new FirebaseClient("https://yatrisewa-4f81e-default-rtdb.asia-southeast1.firebasedatabase.app/");
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using (var scope = _serviceProvider.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                    var devices = await _firebaseClient
                        .Child("devices")
                        .OnceAsync<dynamic>();

                    foreach (var device in devices)
                    {
                        string deviceIdentifier = device.Key;
                        decimal rawLat = (decimal)device.Object.latitude;
                        decimal rawLon = (decimal)device.Object.longitude;
                        decimal speed = (decimal)device.Object.speed;

                        Console.WriteLine($"Device: {deviceIdentifier}, Raw Latitude: {rawLat}, Raw Longitude: {rawLon}, Speed: {speed}");

                        var existingDevice = await context.IoTDevices
                            .Include(d => d.Bus)
                            .FirstOrDefaultAsync(d => d.DeviceIdentifier == deviceIdentifier);

                        // Initialize Kalman filter if not present
                        if (!latFilters.ContainsKey(deviceIdentifier))
                        {
                            latFilters[deviceIdentifier] = new KalmanFilter((double)rawLat);
                            lonFilters[deviceIdentifier] = new KalmanFilter((double)rawLon);
                        }

                        // Apply Kalman Filter for smooth GPS data
                        double filteredLat = latFilters[deviceIdentifier].Update((double)rawLat);
                        double filteredLon = lonFilters[deviceIdentifier].Update((double)rawLon);

                        Console.WriteLine($"Filtered Latitude: {filteredLat}, Filtered Longitude: {filteredLon}");

                        if (existingDevice == null)
                        {
                            var newDevice = new IoTDevice
                            {
                                DeviceIdentifier = deviceIdentifier,
                                DeviceName = "New IoT Device",
                                Latitude = (decimal)filteredLat,
                                Longitude = (decimal)filteredLon,
                                Speed = speed,
                                LastUpdated = DateTime.UtcNow
                            };
                            context.IoTDevices.Add(newDevice);
                        }
                        else
                        {
                            existingDevice.Latitude = (decimal)filteredLat;
                            existingDevice.Longitude = (decimal)filteredLon;
                            existingDevice.Speed = speed;
                            existingDevice.LastUpdated = DateTime.UtcNow;

                            context.IoTDevices.Update(existingDevice);
                        }

                        await context.SaveChangesAsync();

                        // Use BusName if available, otherwise "Unknown Bus"
                        string busName = existingDevice?.Bus?.BusName ?? "Unknown Bus";

                        // Send updated location to frontend via SignalR
                        await _hubContext.Clients.All.SendAsync("UpdateBusLocation", deviceIdentifier, busName, filteredLat, filteredLon, speed);
                    }
                }

                await Task.Delay(5000, stoppingToken); // Reduce delay for real-time updates
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in RealTimeBusLocationService: {ex.Message}");
            }
        }
    }
}
