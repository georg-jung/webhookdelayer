using System.Threading.Channels;

namespace WebhookDelayer;

public static class WebhookChannel
{
    public static Channel<IHeaderDictionary> Instance = Channel.CreateBounded<IHeaderDictionary>(new BoundedChannelOptions(1)
    {
        FullMode = BoundedChannelFullMode.DropOldest,
    });
}
