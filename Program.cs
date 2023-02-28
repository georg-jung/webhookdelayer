using System.Threading.Channels;
using WebhookDelayer;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
builder.Services.AddHostedService<WebhookExecutor>();
var app = builder.Build();

var writer = WebhookChannel.Instance.Writer;

app.MapGet("/", async (HttpRequest req) =>
{
    await writer.WriteAsync(req.Headers);
    return "OK";
});

app.Run();