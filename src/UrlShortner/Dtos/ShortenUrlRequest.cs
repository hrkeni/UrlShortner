using System.ComponentModel.DataAnnotations;

namespace UrlShortner.Dtos;

public record ShortenUrlRequest(
    [Url]
    [Required]
    string LongUrl);
