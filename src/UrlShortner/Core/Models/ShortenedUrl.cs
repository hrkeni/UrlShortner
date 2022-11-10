namespace UrlShortner.Core.Models;


public record ShortenedUrl
{
    public long Id { get; private set; }

    public string LongUrl { get; private set; }

    public string Slug { get; private set; }


}
