using Base62;

namespace UrlShortner.Models;


public record ShortenedUrl
{
    public int Id { get; private set; }

    public string LongUrl { get; private set; }

    public string Slug { get; private set; }

    public ShortenedUrl(int id, string longUrl)
    {
        Id = id;

        if (!Uri.TryCreate(longUrl, UriKind.Absolute, out var _))
        {
            throw new ArgumentException("Invalid URL", nameof(longUrl));
        }

        LongUrl = longUrl;

        var bytes = BitConverter.GetBytes(id);
        Slug = bytes.ToBase62();
    }
}
