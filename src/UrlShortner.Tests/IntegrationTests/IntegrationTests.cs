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

    [Fact]
    public async Task Redirect_WithValidSlug_Should302Redirect()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false});

        var response = await client.PostAsJsonAsync("/shorten", new ShortenUrlRequest("http://this.is.a.test.url/"));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var slug = (await response.Content.ReadFromJsonAsync<ShortenUrlResponse>())?.Slug;

        response = await client.GetAsync("/" + slug);

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.Redirect);
    }

    [Fact]
    public async Task Redirect_WithInvalidSlug_Should404NotFound()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/invalid-slug");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Redirect_WithValidSlug_ShouldIncrementVisitCount()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/shorten", new ShortenUrlRequest("http://testing.visit.counts/"));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var slug = (await response.Content.ReadFromJsonAsync<ShortenUrlResponse>())?.Slug;

        response = await client.GetAsync("/" + slug);

        await Task.Delay(50); // wait a bit for async db update

        var results = await _context.ShortenedUrls.Where(s => s.LongUrl == "http://testing.visit.counts/").ToListAsync();

        results.Count.Should().Be(1);

        results[0].VisitCount.Should().Be(1);
    }

    [Fact]
    public async Task DownloadCsv_ShouldRepondWithCsvFile()
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/shorten", new ShortenUrlRequest("http://testing.visit.counts/"));

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);

        var slug = (await response.Content.ReadFromJsonAsync<ShortenUrlResponse>())?.Slug;

        await client.GetAsync("/" + slug);
        await client.GetAsync("/" + slug);
        await client.GetAsync("/" + slug);

        response = await client.GetAsync("/stats/csv");

        response.StatusCode.Should().Be(System.Net.HttpStatusCode.OK);
        response.Content.Headers!.ContentType!.MediaType.Should().Be("text/csv");
    }

    public void Dispose()
    {
        _scope.Dispose();
    }
}
