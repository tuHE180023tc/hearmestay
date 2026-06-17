using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HearMeStay.Data;
using HearMeStay.Models.Enums;
using HearMeStay.ViewModels;

namespace HearMeStay.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? utm_source, string? utm_medium, string? utm_campaign)
        {
            // Tracking Website Visit
            try 
            {
                var source = utm_source ?? "Direct";
                var userId = User.Identity?.IsAuthenticated == true ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
                var userAgent = Request.Headers["User-Agent"].ToString();

                _context.WebsiteVisitLogs.Add(new HearMeStay.Models.WebsiteVisitLog
                {
                    Source = source,
                    Medium = utm_medium,
                    Campaign = utm_campaign,
                    UserId = userId,
                    IpAddress = ip,
                    UserAgent = userAgent,
                    PageUrl = Request.Path,
                    SessionId = HttpContext.Session?.Id ?? Guid.NewGuid().ToString() // Approximation
                });

                if (!string.IsNullOrEmpty(utm_source))
                {
                    Response.Cookies.Append("hms_utm_source", utm_source, new CookieOptions { Expires = DateTime.Now.AddDays(30) });
                }
                if (!string.IsNullOrEmpty(utm_medium))
                {
                    Response.Cookies.Append("hms_utm_medium", utm_medium, new CookieOptions { Expires = DateTime.Now.AddDays(30) });
                }
                if (!string.IsNullOrEmpty(utm_campaign))
                {
                    Response.Cookies.Append("hms_utm_campaign", utm_campaign, new CookieOptions { Expires = DateTime.Now.AddDays(30) });
                }

                await _context.SaveChangesAsync();
            }
            catch { /* Ignore tracking errors */ }

            // Featured accommodations (approved, active)
            var featured = await _context.Accommodations
                .Where(a => a.Status == AccommodationStatus.Approved && a.IsActive)
                .Include(a => a.Images)
                .Include(a => a.RoomTypes)
                .Include(a => a.Reviews)
                .Take(6)
                .Select(a => new AccommodationCardViewModel
                {
                    Id = a.Id,
                    Name = a.Name,
                    City = a.City,
                    Province = a.Province,
                    AccommodationType = a.AccommodationType,
                    StarRating = a.StarRating,
                    MainImageUrl = a.Images.Where(i => i.IsMain).Select(i => i.ImageUrl).FirstOrDefault() ?? "/images/placeholder-hotel.svg",
                    MinPrice = a.RoomTypes.Any() ? a.RoomTypes.Min(r => r.PricePerNight) : 0,
                    AverageRating = a.Reviews.Any(r => r.IsVisible) ? a.Reviews.Where(r => r.IsVisible).Average(r => r.Rating) : 0,
                    ReviewCount = a.Reviews.Count(r => r.IsVisible),
                    HasQuietRoom = a.RoomTypes.Any(r => r.IsQuietRoom),
                    HasVeganMeal = a.RoomTypes.Any(r => r.SupportsVeganMeal),
                    HasAllergySupport = a.RoomTypes.Any(r => r.SupportsAllergyRequest)
                })
                .ToListAsync();

            return View(featured);
        }

        public IActionResult About() => View();
        public IActionResult Contact() => View();
        public IActionResult BecomePartner() => View();

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() => View(new Models.ErrorViewModel { RequestId = System.Diagnostics.Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
