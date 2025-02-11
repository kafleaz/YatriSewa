using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class BusLocationHub : Hub
{
    // Broadcast a general message to all clients
    public async Task SendMessage(string user, string message)
    {
        await Clients.All.SendAsync("ReceiveMessage", user, message);
    }

    // Notify all clients about a bus location update
    public async Task UpdateBusLocation(string deviceIdentifier, decimal latitude, decimal longitude, decimal speed)
    {
        // Log the update for debugging purposes
        Console.WriteLine($"Updating location for Device: {deviceIdentifier}, Lat: {latitude}, Long: {longitude}, Speed: {speed}");

        // Notify all clients about the update
        await Clients.All.SendAsync("UpdateBusLocation", deviceIdentifier, latitude, longitude, speed);
    }

    // Notify a specific client group about a bus location update
    public async Task UpdateBusLocationForGroup(string groupName, string deviceIdentifier, decimal latitude, decimal longitude, decimal speed)
    {
        await Clients.Group(groupName).SendAsync("UpdateBusLocation", deviceIdentifier, latitude, longitude, speed);
    }

    // Allow clients to join a group (e.g., for specific buses or devices)
    public async Task JoinGroup(string groupName)
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        Console.WriteLine($"Client {Context.ConnectionId} joined group {groupName}");
    }

    // Allow clients to leave a group
    public async Task LeaveGroup(string groupName)
    {
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        Console.WriteLine($"Client {Context.ConnectionId} left group {groupName}");
    }
}