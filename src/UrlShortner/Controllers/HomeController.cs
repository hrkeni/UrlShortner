using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortner.Data;
using UrlShortner.Dtos;
using UrlShortner.Models;

namespace UrlShortner.Controllers;

public class HomeController : Controller
{
    private static readonly Random _random = new();

    private readonly UrlShortnerContext _context;
    private readonly ILogger<HomeController> _logger;

    

    public HomeController(UrlShortnerContext context, ILogger<HomeController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("shorten")]
    public async Task<ActionResult<ShortenUrlResponse>> Shorten(ShortenUrlRequest request, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) {
            return BadRequest(ModelState);
        }

        var shortenedUrl = new ShortenedUrl(_random.Next(), request.LongUrl);

        await _context.ShortenedUrls.AddAsync(shortenedUrl, cancellationToken);
        
        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            return Conflict();
        }

        return Ok(new ShortenUrlResponse(shortenedUrl.Slug));
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult> Redirect(string slug, CancellationToken cancellationToken)
    {
        var shortenedUrl = await _context.ShortenedUrls.FirstOrDefaultAsync(s => s.Slug == slug, cancellationToken);

        if (shortenedUrl == null)
        {
            return NotFound();
        }

        return Redirect(shortenedUrl.LongUrl);
    }

    [HttpGet("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
