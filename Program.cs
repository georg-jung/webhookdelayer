using WebhookDelayer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddLogging();
builder.Services.AddHttpClient();
builder.Services.AddHostedService<WebhookExecutor>();
var app = builder.Build();

var writer = WebhookChannel.Instance.Writer;

async Task<string> OnReq(HttpRequest req) {
    await writer.WriteAsync(req.Headers);
    return "OK";
}

app.MapGet("/{*req}", OnReq);
app.MapPost("/{*req}", OnReq);

app.Run();
