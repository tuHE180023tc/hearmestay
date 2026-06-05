using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HearMeStay.Data;
using HearMeStay.Models;
using HearMeStay.Services.Interfaces;

namespace HearMeStay.Areas.Partner.Controllers
{
    [Area("Partner")][Authorize(Roles = "HotelPartner")]
    public class ReportsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IReportService _reportService;
        public ReportsController(ApplicationDbContext ctx, UserManager<ApplicationUser> um, IReportService rs) { _context = ctx; _userManager = um; _reportService = rs; }

        public async Task<IActionResult> Index(int? accId)
        {
            var userId = _userManager.GetUserId(User);
            var accommodations = await _context.Accommodations.Where(a => a.OwnerId == userId).ToListAsync();
            if (!accommodations.Any()) return View();

            var acc = accId.HasValue ? accommodations.FirstOrDefault(a => a.Id == accId) : accommodations.First();
            if (acc == null) acc = accommodations.First();

            ViewBag.Accommodations = accommodations;
            ViewBag.SelectedAccId = acc.Id;
            ViewBag.AccommodationName = acc.Name;

            var id = acc.Id;
            ViewBag.TotalBookings = await _reportService.GetTotalBookingsAsync(id);
            ViewBag.Revenue = await _reportService.GetTotalRevenueAsync(id);
            ViewBag.Commission = await _reportService.GetTotalCommissionAsync(id);
            ViewBag.ConfirmedRate = await _reportService.GetConfirmedRateAsync(id);
            ViewBag.PreferenceRate = await _reportService.GetPreferenceFormRateAsync(id);
            ViewBag.AvgRating = await _reportService.GetAverageRatingAsync(id);
            ViewBag.AvgPersonalization = await _reportService.GetAveragePersonalizationRatingAsync(id);
            ViewBag.CommonTags = await _reportService.GetCommonTagsAsync(id);
            return View();
        }
    }
}
