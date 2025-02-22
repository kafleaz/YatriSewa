using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

public class PassengerLocationHub : Hub
{
    public async Task SendPassengerLocation(string busId, decimal latitude, decimal longitude)
    {
        // ✅ Send location only to drivers of that specific bus group
        await Clients.Group(busId).SendAsync("ReceivePassengerLocation", latitude, longitude);
    }

    public async Task JoinBusGroup(string busId)
    {
        // ✅ Passenger joins the correct bus group dynamically
        await Groups.AddToGroupAsync(Context.ConnectionId, busId);
    }
}
