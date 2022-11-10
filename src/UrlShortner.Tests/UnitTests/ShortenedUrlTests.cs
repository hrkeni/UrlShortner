using FluentAssertions;
using UrlShortner.Core.Models;

namespace UrlShortner.Tests.UnitTests;

public class ShortenedUrlTests
{
    [Theory]
    [InlineData(0, "0000")]
    [InlineData(-1, "4gfFC3")]
    [InlineData(987654321, "3FQpyc")]
    [InlineData(987654322, "3GZEUs")]
    public void ItShouldEncodeSlugAsBase62WhenCreated(int id, string base62)
    {
        var shortenedUrl = new ShortenedUrl(id, "https://www.google.com");

        shortenedUrl.Slug.Should().NotBeNullOrEmpty();
        shortenedUrl.Slug.Should().Be(base62);
    }

    [Theory]
    [InlineData("https://www.google.com")]
    [InlineData("http://localhost/")]
    [InlineData("http://127.0.0.1/this/is/a/test")]
    [InlineData("https://www.google.com/search?q=url+shortener&oq=google+u&aqs=chrome.0.69i59j69i60l3j0j69i57.1069j0j7&sourceid=chrome&ie=UTF-8")]
    public void ItShouldBeValidWhenUrlIsValid(string url)
    {
        var random = new Random();
        var id = random.Next(int.MinValue, int.MaxValue);

        var shortenedUrl = new ShortenedUrl(id, url);

        shortenedUrl.Should().NotBeNull();
        shortenedUrl.Id.Should().Be(id);
        shortenedUrl.LongUrl.Should().Be(url);
    }

    [Theory]
    [InlineData("www.google.com")]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("/some/relative/url")]
    public void ItShouldThrowAnExceptionWhenUrlIsInalid(string url)
    {
        var random = new Random();
        var id = random.Next(int.MinValue, int.MaxValue);

        var action = () => new ShortenedUrl(id, url);
        action.Should().Throw<ArgumentException>();
    }
}