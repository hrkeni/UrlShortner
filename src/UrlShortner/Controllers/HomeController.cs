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
    private readonly VisitQueue _visitQueue;
    private readonly ILogger<HomeController> _logger;

    

    public HomeController(UrlShortnerContext context, VisitQueue visitQueue, ILogger<HomeController> logger)
    {
        _context = context;
        _visitQueue = visitQueue;
        _logger = logger;
    }

    [HttpGet("")]
    public IActionResult Index()
    {
        return View();
    }

    [HttpPost("shorten")]
    public async Task<ActionResult<ShortenUrlResponse>> Shorten([FromBody] ShortenUrlRequest request, CancellationToken cancellationToken)
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

    [HttpGet("stats")]
    public async Task<ActionResult<ShortenedUrl>> Stats(CancellationToken cancellationToken)
    {
        var shortenedUrls = await _context.ShortenedUrls.ToListAsync(cancellationToken);

        return View(shortenedUrls);
    }

    [HttpGet("{slug}")]
    public async Task<ActionResult> Redirect(string slug, CancellationToken cancellationToken)
    {
        var shortenedUrl = await _context.ShortenedUrls.FirstOrDefaultAsync(s => s.Slug == slug, cancellationToken);

        if (shortenedUrl == null)
        {
            return NotFound();
        }

        var visit = new ShortenedUrlVisit(shortenedUrl.Id, HttpContext.Request.Headers["User-Agent"].ToString(), HttpContext.Connection.RemoteIpAddress?.ToString());

        await _visitQueue.Enqueue(visit);
        
        return Redirect(shortenedUrl.LongUrl);
    }
    

    [HttpGet("error")]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}
