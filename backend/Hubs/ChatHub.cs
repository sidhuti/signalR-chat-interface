using Microsoft.AspNetCore.SignalR;
using SignalRDemo.Services;

namespace SignalRDemo.Hubs;

public class ChatHub : Hub
{
    private readonly LLMResponseService _responseService;
    private readonly ILogger<ChatHub> _logger;
    
    public ChatHub(LLMResponseService responseService, ILogger<ChatHub> logger)
    {
        _responseService = responseService;
        _logger = logger;
    }

    // Send a prompt - uses ConnectionId as session identifier
    public async Task SendPrompt(string prompt)
    {
        // Use connection ID as the session ID (one session per tab/connection)
        string sessionId = Context.ConnectionId;

        // Send immediate acknowledgment
        await Clients.Caller.SendAsync("PromptReceived", prompt);
        
        _logger.LogInformation(
            "[{Time}] Prompt received from connection {ConnectionId}",
            DateTime.Now.ToString("HH:mm:ss"),
            Context.ConnectionId);

        // Queue the request in the background service
        // Using ConnectionId ensures only one active request per tab
        _responseService.QueueRequest(sessionId, prompt, Context.ConnectionId, delaySeconds: 10);
    }

    // Stop/cancel current processing
    public async Task StopGeneration()
    {
        string sessionId = Context.ConnectionId;
        _responseService.CancelRequest(sessionId);
        
        await Clients.Caller.SendAsync("GenerationStopped");
        
        _logger.LogInformation(
            "[{Time}] Generation stopped for connection {ConnectionId}",
            DateTime.Now.ToString("HH:mm:ss"),
            Context.ConnectionId);
    }

    // Connection lifecycle events
    public override async Task OnConnectedAsync()
    {
        await Clients.Caller.SendAsync("Connected", Context.ConnectionId);
        _logger.LogInformation(
            "[{Time}] Client connected: {ConnectionId}",
            DateTime.Now.ToString("HH:mm:ss"),
            Context.ConnectionId);
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        // Cancel any pending request for this connection
        _responseService.CancelRequest(Context.ConnectionId);
        
        _logger.LogInformation(
            "[{Time}] Client disconnected: {ConnectionId}",
            DateTime.Now.ToString("HH:mm:ss"),
            Context.ConnectionId);

        await base.OnDisconnectedAsync(exception);
    }
}
