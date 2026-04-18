using Cerberus.Shared;
using Microsoft.AspNetCore.SignalR;

namespace Cerberus.Backend;

public class PacketEventHub : Hub
{
    public async Task SendPacket(PacketEvent packet)
    {
        await Clients.All.SendAsync("ReceivePacket", packet);
    }

    public async Task ClearPackets()
    {
        await Clients.All.SendAsync("ClearPackets");
    }

    public override async Task OnConnectedAsync()
    {
        await base.OnConnectedAsync();
        Console.WriteLine($"Client connected: {Context.ConnectionId}");
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        await base.OnDisconnectedAsync(exception);
        Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
    }
}
