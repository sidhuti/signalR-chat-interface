using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;

namespace SignalRDemo.Services;

public class LLMResponseService : BackgroundService
{
    private readonly IHubContext<Hubs.ChatHub> _hubContext;
    private readonly ILogger<LLMResponseService> _logger;
    
    // Thread-safe queue for pending sessions
    private readonly ConcurrentQueue<SessionRequest> _pendingRequests = new();
    
    // Track cancellation tokens for each session
    private readonly ConcurrentDictionary<string, CancellationTokenSource> _activeSessions = new();
    
    public LLMResponseService(
        IHubContext<Hubs.ChatHub> hubContext,
        ILogger<LLMResponseService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("LLM Response Service started");

        while (!stoppingToken.IsCancellationRequested)
        {
            // Process any pending requests
            while (_pendingRequests.TryDequeue(out var request))
            {
                // Process each request in a separate task
                _ = ProcessRequestAsync(request, stoppingToken);
            }

            // Check queue every 100ms
            await Task.Delay(100, stoppingToken);
        }
    }

    private async Task ProcessRequestAsync(SessionRequest request, CancellationToken appCancellationToken)
    {
        // Create cancellation token source for this specific request
        var cts = CancellationTokenSource.CreateLinkedTokenSource(appCancellationToken);
        _activeSessions[request.SessionId] = cts;

        try
        {
            _logger.LogInformation(
                "[{Time}] Processing prompt for connection {ConnectionId} - Will respond in {Delay} seconds",
                DateTime.Now.ToString("HH:mm:ss"),
                request.ConnectionId,
                request.DelaySeconds);

            // Wait for the specified delay
            await Task.Delay(TimeSpan.FromSeconds(request.DelaySeconds), cts.Token);

            // Generate response
            var response = GenerateLLMResponse(request.Prompt);

            // Send response to specific client
            await _hubContext.Clients.Client(request.ConnectionId)
                .SendAsync("ReceiveResponse", request.Prompt, response, cts.Token);

            _logger.LogInformation(
                "[{Time}] Response sent to connection {ConnectionId}",
                DateTime.Now.ToString("HH:mm:ss"),
                request.ConnectionId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation(
                "[{Time}] Request cancelled for connection {ConnectionId}",
                DateTime.Now.ToString("HH:mm:ss"),
                request.ConnectionId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "[{Time}] Error processing request for connection {ConnectionId}: {Message}",
                DateTime.Now.ToString("HH:mm:ss"),
                request.ConnectionId,
                ex.Message);
                
            // Send error to client
            try
            {
                await _hubContext.Clients.Client(request.ConnectionId)
                    .SendAsync("Error", "An error occurred while processing your request.");
            }
            catch { /* Client might be disconnected */ }
        }
        finally
        {
            // Clean up
            _activeSessions.TryRemove(request.SessionId, out _);
            cts.Dispose();
        }
    }

    public void QueueRequest(string sessionId, string prompt, string connectionId, int delaySeconds = 120)
    {
        // Cancel any existing request for this session (only one active request per connection)
        CancelRequest(sessionId);

        var request = new SessionRequest
        {
            SessionId = sessionId,
            Prompt = prompt,
            ConnectionId = connectionId,
            DelaySeconds = delaySeconds,
            QueuedAt = DateTime.UtcNow
        };

        _pendingRequests.Enqueue(request);
        
        _logger.LogInformation(
            "[{Time}] Queued request for connection {ConnectionId}",
            DateTime.Now.ToString("HH:mm:ss"),
            connectionId);
    }

    public void CancelRequest(string sessionId)
    {
        if (_activeSessions.TryGetValue(sessionId, out var cts))
        {
            cts.Cancel();
            _logger.LogInformation(
                "[{Time}] Cancelled request for session {SessionId}",
                DateTime.Now.ToString("HH:mm:ss"),
                sessionId);
        }
    }

    private string GenerateLLMResponse(string prompt)
    {
        return $"I've analyzed your prompt: \"{prompt}\"\n\n" +
               $"This is a simulated response from an LLM. In a real application, this would be " +
               $"the output from GPT-4, Claude, Gemini, or another language model.\n\n" +
               $"The response considers your question and provides a thoughtful answer. " +
               $"It took about 10 seconds to process, simulating the time an actual LLM might take.\n\n" +
               $"[Generated at {DateTime.Now:HH:mm:ss}]";
    }
}

public class SessionRequest
{
    public string SessionId { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public string ConnectionId { get; set; } = string.Empty;
    public int DelaySeconds { get; set; }
    public DateTime QueuedAt { get; set; }
}
