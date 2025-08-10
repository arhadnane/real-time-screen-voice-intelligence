using Microsoft.AspNetCore.SignalR;

namespace RealTimeIntelligence.Web.Hubs;

public class RealTimeHub : Hub
{
	public override async Task OnConnectedAsync()
	{
		await base.OnConnectedAsync();
		await Clients.Caller.SendAsync("Connected", DateTime.UtcNow);
	}

	public Task Subscribe(string channel) => Groups.AddToGroupAsync(Context.ConnectionId, channel);
	public Task Unsubscribe(string channel) => Groups.RemoveFromGroupAsync(Context.ConnectionId, channel);
}
