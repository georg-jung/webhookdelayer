using System.Threading.Channels;
using Microsoft.Extensions.Primitives;

namespace WebhookDelayer;

public static class WebhookChannel
{
    public static Channel<(DateTimeOffset Timestamp, IDictionary<string, StringValues> Headers)> Instance = Channel.CreateBounded<(DateTimeOffset, IDictionary<string, StringValues>)>(new BoundedChannelOptions(1)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
    });
}
