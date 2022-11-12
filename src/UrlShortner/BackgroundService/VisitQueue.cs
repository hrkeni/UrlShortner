using System.Threading.Channels;

public class VisitQueue
{
    
    private readonly Channel<ShortenedUrlVisit> _queue;

    public VisitQueue(int capacity)
    {
        var options = new BoundedChannelOptions(capacity)
        {
            FullMode = BoundedChannelFullMode.Wait
        };
        _queue = Channel.CreateBounded<ShortenedUrlVisit>(options);
    }

    public async ValueTask Enqueue(ShortenedUrlVisit workItem)
    {
        if (workItem == null)
        {
            throw new ArgumentNullException(nameof(workItem));
        }

        await _queue.Writer.WriteAsync(workItem);
    }

    public async ValueTask<ShortenedUrlVisit> Dequeue(CancellationToken cancellationToken)
    {
        var workItem = await _queue.Reader.ReadAsync(cancellationToken);

        return workItem;
    }
}