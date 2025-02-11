using Firebase.Database;
using Firebase.Database.Query;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;
using YatriSewa.Models;

public class RealTimeBusLocationService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider; // Use IServiceProvider instead of directly injecting ApplicationContext
    private readonly IHubContext<BusLocationHub> _hubContext;
    private readonly FirebaseClient _firebaseClient;

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
                using (var scope = _serviceProvider.CreateScope()) // Create a new scope
                {
                    var context = scope.ServiceProvider.GetRequiredService<ApplicationContext>();

                    var devices = await _firebaseClient
                        .Child("devices")
                        .OnceAsync<dynamic>();

                    foreach (var device in devices)
                    {
                        string deviceIdentifier = device.Key;
                        decimal latitude = (decimal)device.Object.latitude;
                        decimal longitude = (decimal)device.Object.longitude;
                        decimal speed = (decimal)device.Object.speed;
                        Console.WriteLine($"Device: {deviceIdentifier}, Latitude: {latitude}, Longitude: {longitude}, Speed: {speed}");
                        var existingDevice = await context.IoTDevices
                            .FirstOrDefaultAsync(d => d.DeviceIdentifier == deviceIdentifier);

                        if (existingDevice == null)
                        {
                            //context.IoTDevices.Add(new IoTDevice
                            //{
                            //    DeviceName = "New IoT Device",
                            //    DeviceIdentifier = deviceIdentifier,
                            //    Latitude = latitude,
                            //    Longitude = longitude,
                            //    Speed = speed,
                            //    LastUpdated = DateTime.UtcNow
                            //});
                            var newDevice = new IoTDevice
                            {
                                DeviceIdentifier = deviceIdentifier,
                                DeviceName = "New IoT Device", // Or assign dynamically
                                Latitude = latitude,
                                Longitude = longitude,
                                Speed = speed,
                                LastUpdated = DateTime.UtcNow
                            };
                            context.IoTDevices.Add(newDevice);
                        }
                        else
                        {
                            existingDevice.Latitude = latitude;
                            existingDevice.Longitude = longitude;
                            existingDevice.Speed = speed;
                            existingDevice.LastUpdated = DateTime.UtcNow;

                            context.IoTDevices.Update(existingDevice);
                        }

                        await context.SaveChangesAsync();

                        await _hubContext.Clients.All.SendAsync("UpdateBusLocation", deviceIdentifier, latitude, longitude, speed);
                    }
                }

                await Task.Delay(10000, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in RealTimeBusLocationService: {ex.Message}");
            }
        }
    }
}

