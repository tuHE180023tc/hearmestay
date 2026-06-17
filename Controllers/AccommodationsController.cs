using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using HearMeStay.Data;
using HearMeStay.Models.Enums;
using HearMeStay.ViewModels;

namespace HearMeStay.Controllers
{
    public class AccommodationsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public AccommodationsController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(AccommodationSearchViewModel search)
        {
            var query = _context.Accommodations
                .Where(a => a.Status == AccommodationStatus.Approved && a.IsActive)
                .Include(a => a.Images)
                .Include(a => a.RoomTypes)
                .Include(a => a.Reviews)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search.City))
                query = query.Where(a => a.City.Contains(search.City) || a.Province.Contains(search.City));

            if (search.AccommodationType.HasValue)
                query = query.Where(a => a.AccommodationType == search.AccommodationType);

            if (search.PriceMin.HasValue)
                query = query.Where(a => a.RoomTypes.Any(r => r.PricePerNight >= search.PriceMin));

            if (search.PriceMax.HasValue)
                query = query.Where(a => a.RoomTypes.Any(r => r.PricePerNight <= search.PriceMax));

            if (search.Guests.HasValue)
                query = query.Where(a => a.RoomTypes.Any(r => r.Capacity >= search.Guests));

            if (search.IsQuietRoom == true)
                query = query.Where(a => a.RoomTypes.Any(r => r.IsQuietRoom));

            if (search.SupportsVeganMeal == true)
                query = query.Where(a => a.RoomTypes.Any(r => r.SupportsVeganMeal));

            if (search.SupportsAllergyRequest == true)
                query = query.Where(a => a.RoomTypes.Any(r => r.SupportsAllergyRequest));

            if (search.NoStrongScentAvailable == true)
                query = query.Where(a => a.RoomTypes.Any(r => r.NoStrongScentAvailable));

            if (search.HasPrivateBathroom == true)
                query = query.Where(a => a.RoomTypes.Any(r => r.HasPrivateBathroom));

            search.Results = await query.Select(a => new AccommodationCardViewModel
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
                HasAllergySupport = a.RoomTypes.Any(r => r.SupportsAllergyRequest),
                HasPrivateBathroom = a.RoomTypes.Any(r => r.HasPrivateBathroom),
                NoStrongScentAvailable = a.RoomTypes.Any(r => r.NoStrongScentAvailable)
            }).ToListAsync();

            return View(search);
        }

        public async Task<IActionResult> Details(int id)
        {
            var acc = await _context.Accommodations
                .AsSplitQuery()
                .Include(a => a.Images)
                .Include(a => a.RoomTypes).ThenInclude(r => r.Images)
                .Include(a => a.AccommodationAmenities).ThenInclude(aa => aa.Amenity)
                .Include(a => a.AddOnServices.Where(s => s.IsActive))
                .Include(a => a.Reviews.Where(r => r.IsVisible)).ThenInclude(r => r.User)
                .FirstOrDefaultAsync(a => a.Id == id && a.Status == AccommodationStatus.Approved && a.IsActive);

            if (acc == null) return NotFound();

            try
            {
                var source = Request.Cookies["hms_utm_source"] ?? "Direct";
                var userId = User.Identity?.IsAuthenticated == true ? User.FindFirstValue(ClaimTypes.NameIdentifier) : null;
                _context.AccommodationViewLogs.Add(new HearMeStay.Models.AccommodationViewLog
                {
                    AccommodationId = id,
                    UserId = userId,
                    Source = source,
                    SessionId = HttpContext.Session?.Id ?? Guid.NewGuid().ToString()
                });
                await _context.SaveChangesAsync();
            }
            catch { /* Ignore tracking errors */ }

            return View(acc);
        }
    }
}
