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
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBookingService _bookingService;

        public BookingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IBookingService bookingService)
        {
            _context = context;
            _userManager = userManager;
            _bookingService = bookingService;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var bookings = await _context.Bookings
                .Where(b => b.Accommodation.OwnerId == userId)
                .Include(b => b.Accommodation).Include(b => b.RoomType).Include(b => b.User)
                .OrderByDescending(b => b.CreatedAt).ToListAsync();
            return View(bookings);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .Include(b => b.Accommodation).Include(b => b.RoomType).Include(b => b.User)
                .Include(b => b.GuestPreference)
                    .ThenInclude(p => p!.GuestInsight)
                        .ThenInclude(gi => gi!.Tags)
                .Include(b => b.GuestPreference)
                    .ThenInclude(p => p!.GuestInsight)
                        .ThenInclude(gi => gi!.Tasks)
                .Include(b => b.GuestPreference)
                    .ThenInclude(p => p!.GuestInsight)
                        .ThenInclude(gi => gi!.UpsellSuggestions)
                .FirstOrDefaultAsync(b => b.Id == id && b.Accommodation.OwnerId == userId);
            if (booking == null) return NotFound();
            return View(booking);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id, HearMeStay.Models.Enums.SpecialRequestStatus specialRequestStatus, string? partnerSpecialRequestNote, decimal extraFee = 0)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.Include(b => b.Accommodation).Include(b => b.GuestPreference).FirstOrDefaultAsync(b => b.Id == id && b.Accommodation.OwnerId == userId);
            if (booking == null) return NotFound();
            
            // Confirm the booking
            await _bookingService.ConfirmBookingAsync(id, partnerSpecialRequestNote, extraFee);
            
            // Handle Special Request Status if a GuestPreference exists
            if (booking.GuestPreference != null)
            {
                booking.GuestPreference.SpecialRequestStatus = specialRequestStatus;
                booking.GuestPreference.PartnerSpecialRequestNote = partnerSpecialRequestNote;
                _context.Update(booking.GuestPreference);
                await _context.SaveChangesAsync();
            }

            TempData["Success"] = "Booking đã được xác nhận và phản hồi khách hàng.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? partnerNote)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.Include(b => b.Accommodation).FirstOrDefaultAsync(b => b.Id == id && b.Accommodation.OwnerId == userId);
            if (booking == null) return NotFound();
            await _bookingService.RejectBookingAsync(id, partnerNote);
            TempData["Success"] = "Đã từ chối đặt phòng.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> VerifyPayment(int id)
        {
            var userId = _userManager.GetUserId(User);
            var user = await _userManager.FindByIdAsync(userId);
            var booking = await _context.Bookings.Include(b => b.Accommodation).FirstOrDefaultAsync(b => b.Id == id && b.Accommodation.OwnerId == userId);
            if (booking == null) return NotFound();

            await _bookingService.VerifyPaymentAsync(id, user?.FullName ?? "Partner");
            TempData["Success"] = "Đã xác minh thanh toán thành công. Đặt phòng đã hoàn tất.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectPayment(int id, string reason)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.Include(b => b.Accommodation).FirstOrDefaultAsync(b => b.Id == id && b.Accommodation.OwnerId == userId);
            if (booking == null) return NotFound();

            await _bookingService.RejectPaymentAsync(id, reason);
            TempData["Success"] = "Đã báo cáo chưa nhận được thanh toán. Trạng thái đã được chuyển lại thành Chờ thanh toán.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkCompleted(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.Include(b => b.Accommodation).FirstOrDefaultAsync(b => b.Id == id && b.Accommodation.OwnerId == userId);
            if (booking == null) return NotFound();
            await _bookingService.MarkCompletedAsync(id);
            TempData["Success"] = "Lưu trú đã hoàn tất.";
            return RedirectToAction("Index");
        }
    }
}
