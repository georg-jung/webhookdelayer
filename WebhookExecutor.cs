namespace WebhookDelayer;

public sealed class WebhookExecutor : IHostedService, IDisposable
{
    private readonly CancellationTokenSource cts = new();
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ILogger<WebhookExecutor> _logger;

    public WebhookExecutor(IHttpClientFactory httpClientFactory, ILogger<WebhookExecutor> logger)
    {
        _httpClientFactory = httpClientFactory;
        _logger = logger;
    }

    public void Dispose()
    {
        cts.Dispose();
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        const string WebhookTarget = "WEBHOOK_TARGET";
        const string WebhookDelay = "WEBHOOK_DELAY";
        var hookTarget = Environment.GetEnvironmentVariable(WebhookTarget)
            ?? throw new InvalidOperationException($"{WebhookTarget} not set.");
        var delayStr = Environment.GetEnvironmentVariable(WebhookDelay);
        var parsed = int.TryParse(delayStr, out var delay);
        if (!parsed) throw new InvalidOperationException($"{WebhookDelay} not set.");

        Task.Run(async () =>
        {
            var r = WebhookChannel.Instance.Reader;
            while (!cancellationToken.IsCancellationRequested)
            {
                var (timestamp, headers) = await r.ReadAsync(cancellationToken);

                // Wait for <delay> msec if another request comes in.
                // If yes, use the newer one and skip this one.
                await Task.Delay((int)Math.Max(0, delay - (DateTimeOffset.Now - timestamp).TotalMilliseconds));
                if (r.Count > 0)
                {
                    _logger.LogInformation("Request skipped due to newer request in queue.");
                    continue;
                }

                using var hc = _httpClientFactory.CreateClient();
                var req = new HttpRequestMessage(HttpMethod.Get, hookTarget);
                foreach (var (k, v) in headers)
                {
                    if ("Host".Equals(k, StringComparison.OrdinalIgnoreCase)) continue;
                    req.Headers.Add(k, (IEnumerable<string>)v);
                }

                try
                {
                    var rx = await hc.SendAsync(req, cancellationToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error while sending webhook");
                }
            }
        }, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cts.Cancel();
        return Task.CompletedTask;
    }
}
