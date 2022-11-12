using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using UrlShortner.Data;
using UrlShortner.Dtos;

namespace UrlShortner.Tests.IntegrationTests;

public class IntegrationTests : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly IServiceScope _scope;
    private readonly UrlShortnerContext _context;

    public IntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
        
        // if running outside of container
        if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable("CUSTOMCONNSTR_URLSHORTNER")))
        {
            Environment.SetEnvironmentVariable("CUSTOMCONNSTR_URLSHORTNER", "Host=localhost;Database=url_shortner_test;Username=postgres;Password=passw0rd!");
        }

        _scope = factory.Services.CreateScope();
        
        _context = _scope.ServiceProvider.GetRequiredService<UrlShortnerContext>();
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();
        
    }

    [Fact]
    public async Task Shorten_WithValidLongUrl_ShouldReturnSlugAndSaveToDatabase()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/shorten", new ShortenUrlRequest("http://this.is.a.test.url/"));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var results = await _context.ShortenedUrls.Where(s => s.LongUrl == "http://this.is.a.test.url/").ToListAsync();

        results.Count.Should().Be(1);

        results[0].Slug.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Shorten_WithInvalidLongUrl_ShouldReturnBadRequest()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/shorten", new ShortenUrlRequest("bad.long.url"));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.BadRequest);

        var results = await _context.ShortenedUrls.ToListAsync();

        results.Count.Should().Be(0);
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
