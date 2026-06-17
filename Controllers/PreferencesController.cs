using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HearMeStay.Data;
using HearMeStay.Models;
using HearMeStay.Models.Enums;
using HearMeStay.ViewModels;
using HearMeStay.Services.Interfaces;

namespace HearMeStay.Controllers
{
    [Authorize(Roles = "Traveler")]
    public class PreferencesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IPreferenceAnalysisService _analysisService;
        private readonly INotificationService _notificationService;
        private readonly IBookingOperationLogService _operationLogService;

        public PreferencesController(ApplicationDbContext context, UserManager<ApplicationUser> userManager,
            IPreferenceAnalysisService analysisService, INotificationService notificationService, IBookingOperationLogService operationLogService)
        {
            _context = context;
            _userManager = userManager;
            _analysisService = analysisService;
            _notificationService = notificationService;
            _operationLogService = operationLogService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .Include(b => b.Accommodation)
                .Include(b => b.GuestPreference)
                .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

            if (booking == null) return NotFound();
            if (booking.BookingStatus != BookingStatus.Confirmed && booking.BookingStatus != BookingStatus.Pending)
            {
                TempData["Error"] = "Không thể điền form nhu cầu cho đặt phòng này.";
                return RedirectToAction("Details", "Bookings", new { id = bookingId });
            }
            if (booking.GuestPreference != null)
                return RedirectToAction("Details", new { bookingId });

            var profile = await _context.UserPreferenceProfiles.FirstOrDefaultAsync(p => p.UserId == userId && p.IsActive);

            var model = new GuestPreferenceCreateViewModel
            {
                BookingId = booking.Id,
                BookingCode = booking.BookingCode,
                AccommodationName = booking.Accommodation.Name,
                SavedProfile = profile
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GuestPreferenceCreateViewModel model)
        {
            if (!model.ConsentToShareWithHotel)
            {
                ModelState.AddModelError("ConsentToShareWithHotel", "Vui lòng đồng ý chia sẻ thông tin với nơi lưu trú để tiếp tục.");
                var b = await _context.Bookings.Include(x => x.Accommodation).FirstOrDefaultAsync(x => x.Id == model.BookingId);
                if (b != null) { model.BookingCode = b.BookingCode; model.AccommodationName = b.Accommodation.Name; }
                return View(model);
            }

            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.Include(x => x.Accommodation).FirstOrDefaultAsync(b => b.Id == model.BookingId && b.UserId == userId);
            if (booking == null || (booking.BookingStatus != BookingStatus.Confirmed && booking.BookingStatus != BookingStatus.Pending)) return NotFound();

            var pref = new GuestPreference
            {
                BookingId = model.BookingId,
                RawText = model.RawText,
                HasFoodAllergy = model.HasFoodAllergy,
                FoodAllergyDetail = model.FoodAllergyDetail,
                DietPreference = model.DietPreference,
                RoomPreference = model.RoomPreference,
                HealthNote = model.HealthNote,
                SpecialOccasion = model.SpecialOccasion,
                TravelPurpose = model.TravelPurpose,
                ActivityInterest = model.ActivityInterest,
                NeedAirportPickup = model.NeedAirportPickup,
                NeedEarlyCheckIn = model.NeedEarlyCheckIn,
                NeedDecoration = model.NeedDecoration,
                ConsentToShareWithHotel = model.ConsentToShareWithHotel
            };

            _context.GuestPreferences.Add(pref);
            await _context.SaveChangesAsync();
            
            await _operationLogService.LogStatusChangeAsync(booking.Id, userId, "Traveler", booking.BookingStatus, booking.BookingStatus, HearMeStay.Models.Enums.BookingOperationActionType.PreferenceSubmitted, "Khách đã gửi nhu cầu cá nhân/special request.");

            // Run AI analysis
            await _analysisService.AnalyzePreferenceAsync(pref);
            await _operationLogService.LogSystemActionAsync(booking.Id, HearMeStay.Models.Enums.BookingOperationActionType.AiAnalyzed, "AI đã phân tích nhu cầu khách thành tag/task.");

            // Notify partner
            await _notificationService.CreateNotificationAsync(
                booking.Accommodation.OwnerId,
                "Khách đã chia sẻ nhu cầu cá nhân",
                $"Khách đặt phòng #{booking.BookingCode} đã điền form nhu cầu. Xem Guest Insight để chuẩn bị.",
                "PreferenceSubmitted");
                
            await _operationLogService.LogSystemActionAsync(booking.Id, HearMeStay.Models.Enums.BookingOperationActionType.SentToHotelPartner, "Booking đã được gửi đến nơi lưu trú để xác nhận.");

            // Xử lý lưu hồ sơ sở thích
            if (model.SaveToProfile)
            {
                var profile = await _context.UserPreferenceProfiles.FirstOrDefaultAsync(p => p.UserId == userId);
                if (profile == null)
                {
                    profile = new UserPreferenceProfile { UserId = userId, IsActive = true };
                    _context.UserPreferenceProfiles.Add(profile);
                }

                var raw = model.RawText ?? "";
                
                var foodPrefs = new List<string>();
                if (raw.Contains("Tôi ăn chay")) foodPrefs.Add("Tôi ăn chay");
                if (raw.Contains("Tôi không ăn đồ cay")) foodPrefs.Add("Tôi không ăn đồ cay");
                if (raw.Contains("Tôi cần bữa sáng phù hợp")) foodPrefs.Add("Tôi cần bữa sáng phù hợp");
                profile.FoodPreferences = foodPrefs.Any() ? string.Join(", ", foodPrefs) : null;

                if (raw.Contains("Tôi dị ứng hải sản")) profile.AllergyNotes = "Tôi dị ứng hải sản";

                var roomPrefs = new List<string>();
                if (raw.Contains("Tôi muốn phòng yên tĩnh")) roomPrefs.Add("Tôi muốn phòng yên tĩnh");
                if (raw.Contains("Tôi không thích mùi hương mạnh")) roomPrefs.Add("Tôi không thích mùi hương mạnh");
                if (raw.Contains("Tôi muốn giường lớn")) roomPrefs.Add("Tôi muốn giường lớn");
                if (raw.Contains("Tôi muốn phòng có ánh sáng tự nhiên")) roomPrefs.Add("Tôi muốn phòng có ánh sáng tự nhiên");
                profile.RoomPreferences = roomPrefs.Any() ? string.Join(", ", roomPrefs) : null;
                
                var services = new List<string>();
                if (raw.Contains("Cần trang trí phòng")) services.Add("Cần trang trí phòng");
                if (raw.Contains("Cần đưa đón sân bay")) services.Add("Cần đưa đón sân bay");
                if (raw.Contains("Muốn ăn BBQ")) services.Add("Muốn ăn BBQ");
                if (raw.Contains("Cần Spa")) services.Add("Cần Spa");
                if (raw.Contains("Muốn đi tour địa phương")) services.Add("Muốn đi tour địa phương");
                profile.ServicePreferences = services.Any() ? string.Join(", ", services) : null;
                
                profile.ActivityInterests = null;
                profile.ConsentToShareWithHotel = model.ConsentToShareWithHotel;

                if (model.ConsentToStoreHealthNotes)
                {
                    profile.HealthNotes = model.HealthNote;
                    profile.ConsentToStoreHealthNotes = true;
                }
                else
                {
                    profile.HealthNotes = null;
                    profile.ConsentToStoreHealthNotes = false;
                }

                profile.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Thông tin của bạn đã được gửi. Nơi lưu trú sẽ chuẩn bị dựa trên nhu cầu phù hợp.";
            return RedirectToAction("Details", new { bookingId = model.BookingId });
        }

        public async Task<IActionResult> Details(int bookingId)
        {
            var userId = _userManager.GetUserId(User);
            var pref = await _context.GuestPreferences
                .Include(p => p.Booking).ThenInclude(b => b.Accommodation)
                .Include(p => p.GuestInsight).ThenInclude(gi => gi!.Tags)
                .FirstOrDefaultAsync(p => p.Booking.Id == bookingId && p.Booking.UserId == userId);

            if (pref == null) return NotFound();
            return View(pref);
        }
    }
}
