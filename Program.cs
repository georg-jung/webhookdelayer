using WebhookDelayer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<WebhookExecutor>();
var app = builder.Build();

var writer = WebhookChannel.Instance.Writer;

async Task<string> OnReq(HttpRequest req, ILoggerFactory logFac) {
    var log = logFac.CreateLogger(nameof(OnReq));

    // We need to create a copy of the header dictionary to avoid intermittent disposal
    await writer.WriteAsync((DateTimeOffset.Now, req.Headers.ToDictionary(kvp => kvp.Key, kvp => kvp.Value)));
    log.LogInformation("Handled incoming webhook");
    return "OK";
}

app.MapGet("/{*req}", OnReq);
app.MapPost("/{*req}", OnReq);

app.Run();
