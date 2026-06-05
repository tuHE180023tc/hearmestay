using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HearMeStay.Data;
using HearMeStay.Models;
using HearMeStay.Models.Enums;
using HearMeStay.Services.Interfaces;

namespace HearMeStay.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Authorize(Roles = "HotelPartner")]
    public class DashboardController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReportService _reportService;

        public DashboardController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IReportService reportService)
        {
            _context = context;
            _userManager = userManager;
            _reportService = reportService;
        }

        public async Task<IActionResult> Index(int? accId)
        {
            var userId = _userManager.GetUserId(User);
            var accommodations = await _context.Accommodations.Where(a => a.OwnerId == userId).ToListAsync();
            if (!accommodations.Any())
            {
                ViewBag.NoAccommodation = true;
                return View();
            }

            var accommodation = accId.HasValue ? accommodations.FirstOrDefault(a => a.Id == accId) : accommodations.First();
            if (accommodation == null) accommodation = accommodations.First();

            ViewBag.Accommodations = accommodations;
            ViewBag.SelectedAccId = accommodation.Id;

            var id = accommodation.Id;
            ViewBag.AccommodationName = accommodation.Name;
            ViewBag.AccommodationStatus = accommodation.Status;
            ViewBag.PendingBookings = await _context.Bookings.CountAsync(b => b.AccommodationId == id && b.BookingStatus == BookingStatus.Pending);
            ViewBag.ConfirmedBookings = await _context.Bookings.CountAsync(b => b.AccommodationId == id && (b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Completed));
            ViewBag.UpcomingCheckIns = await _context.Bookings.CountAsync(b => b.AccommodationId == id && b.BookingStatus == BookingStatus.Confirmed && b.CheckInDate <= DateTime.Now.AddDays(7) && b.CheckInDate >= DateTime.Now);
            ViewBag.HighPriorityInsights = await _context.GuestInsights.CountAsync(gi => gi.GuestPreference.Booking.AccommodationId == id && (gi.PriorityLevel == PriorityLevel.High || gi.PriorityLevel == PriorityLevel.Critical));
            ViewBag.Revenue = await _reportService.GetTotalRevenueAsync(id);
            ViewBag.AvgRating = await _reportService.GetAverageRatingAsync(id);
            ViewBag.PreferenceRate = await _reportService.GetPreferenceFormRateAsync(id);

            return View();
        }
    }
}
