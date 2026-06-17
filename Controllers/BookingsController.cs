using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HearMeStay.Data;
using HearMeStay.Models;
using HearMeStay.ViewModels;
using HearMeStay.Services.Interfaces;
using HearMeStay.Models.Enums;

namespace HearMeStay.Controllers
{
    [Authorize(Roles = "Traveler")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IBookingService _bookingService;
        private readonly IBookingOperationLogService _operationLogService;
        private readonly IPdfExportService _pdfService;

        public BookingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IBookingService bookingService, IBookingOperationLogService operationLogService, IPdfExportService pdfService)
        {
            _context = context;
            _userManager = userManager;
            _bookingService = bookingService;
            _operationLogService = operationLogService;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<IActionResult> Create(int roomTypeId)
        {
            var roomType = await _context.RoomTypes.Include(r => r.Accommodation).FirstOrDefaultAsync(r => r.Id == roomTypeId);
            if (roomType == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            var model = new BookingCreateViewModel
            {
                AccommodationId = roomType.AccommodationId,
                AccommodationName = roomType.Accommodation.Name,
                RoomTypeId = roomType.Id,
                RoomTypeName = roomType.Name,
                PricePerNight = roomType.PricePerNight,
                GuestFullName = user?.FullName ?? "",
                GuestEmail = user?.Email ?? "",
                CheckInDate = DateTime.Today.AddDays(1),
                CheckOutDate = DateTime.Today.AddDays(2)
            };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BookingCreateViewModel model, bool skipPreferences = false)
        {
            if (model.CheckOutDate <= model.CheckInDate)
                ModelState.AddModelError("CheckOutDate", "Ngày trả phòng phải sau ngày nhận phòng.");

            if (!ModelState.IsValid)
            {
                var rt = await _context.RoomTypes.Include(r => r.Accommodation).FirstOrDefaultAsync(r => r.Id == model.RoomTypeId);
                if (rt != null) { model.AccommodationName = rt.Accommodation.Name; model.RoomTypeName = rt.Name; model.PricePerNight = rt.PricePerNight; }
                return View(model);
            }

            var user = await _userManager.GetUserAsync(User);
            var booking = new Booking
            {
                UserId = user!.Id,
                AccommodationId = model.AccommodationId,
                RoomTypeId = model.RoomTypeId,
                GuestFullName = model.GuestFullName,
                GuestEmail = model.GuestEmail,
                GuestPhone = model.GuestPhone,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                NumberOfGuests = model.NumberOfGuests,
                NumberOfRooms = model.NumberOfRooms,
                GuestNote = model.GuestNote,
                MarketingSource = Request.Cookies["hms_utm_source"] ?? "Direct",
                MarketingMedium = Request.Cookies["hms_utm_medium"],
                MarketingCampaign = Request.Cookies["hms_utm_campaign"]
            };

            await _bookingService.CreateBookingAsync(booking);
            
            await _operationLogService.LogStatusChangeAsync(booking.Id, user.Id, "Traveler", null, BookingStatus.Pending, HearMeStay.Models.Enums.BookingOperationActionType.BookingCreated, "Khách đã gửi yêu cầu đặt phòng.", "Chờ xác nhận");
            
            if (skipPreferences)
            {
                TempData["Success"] = "Đặt phòng thành công! Bạn có thể xem chi tiết tại đây.";
                return RedirectToAction("MyBookings");
            }
            
            return RedirectToAction("Create", "Preferences", new { bookingId = booking.Id });
        }

        public async Task<IActionResult> MyBookings()
        {
            var userId = _userManager.GetUserId(User);
            var bookings = await _context.Bookings
                .Where(b => b.UserId == userId)
                .Include(b => b.Accommodation)
                .Include(b => b.RoomType)
                .Include(b => b.GuestPreference)
                .Include(b => b.Review)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            return View(bookings);
        }

        public async Task<IActionResult> Details(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings
                .Include(b => b.Accommodation)
                .Include(b => b.RoomType)
                .Include(b => b.GuestPreference).ThenInclude(p => p!.GuestInsight)
                .Include(b => b.Review)
                .Include(b => b.OperationLogs.OrderByDescending(l => l.CreatedAt))
                .FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (booking == null) return NotFound();

            if (booking.BookingStatus == BookingStatus.PaymentPending)
            {
                await _operationLogService.LogSystemActionAsync(booking.Id, HearMeStay.Models.Enums.BookingOperationActionType.PaymentOpened, "Khách đã mở trang thanh toán.");
            }

            return View(booking);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitPayment(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (booking == null) return NotFound();

            var result = await _bookingService.SubmitPaymentProofAsync(id, booking.PaymentTransferContent ?? "");
            if (result == null)
            {
                TempData["Error"] = "Không thể báo cáo thanh toán cho đặt phòng này.";
                return RedirectToAction("Details", new { id });
            }

            await _operationLogService.LogStatusChangeAsync(id, userId, "Traveler", BookingStatus.PaymentPending, BookingStatus.PaymentVerificationPending, HearMeStay.Models.Enums.BookingOperationActionType.PaymentSubmitted, "Khách đã báo thanh toán.", "Admin hoặc Partner cần xác minh thanh toán.");

            TempData["Success"] = "Đã báo cáo thanh toán. Vui lòng chờ hệ thống xác minh.";
            return RedirectToAction("Details", new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (booking == null) return NotFound();

            var result = await _bookingService.CancelBookingAsync(id);
            if (result == null)
            {
                TempData["Error"] = "Không thể hủy đặt phòng này.";
                return RedirectToAction("Details", new { id });
            }

            await _operationLogService.LogStatusChangeAsync(id, userId, "Traveler", booking.BookingStatus, BookingStatus.Cancelled, HearMeStay.Models.Enums.BookingOperationActionType.BookingCancelled, "Khách đã hủy đặt phòng.");

            TempData["Success"] = "Đã hủy đặt phòng thành công.";
            return RedirectToAction("MyBookings");
        }

        [HttpGet]
        public async Task<IActionResult> ExportBookingConfirmationPdf(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (booking == null) return NotFound();
            var pdfBytes = await _pdfService.GenerateBookingConfirmationPdfAsync(id);
            if (pdfBytes == null) return NotFound("Lỗi tạo PDF.");
            return File(pdfBytes, "application/pdf", $"BookingConfirmation_HMS{id:D5}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ExportPaymentReceiptPdf(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == id && b.UserId == userId);
            if (booking == null) return NotFound();
            var pdfBytes = await _pdfService.GeneratePaymentReceiptPdfAsync(id);
            if (pdfBytes == null) return NotFound("Lỗi tạo PDF.");
            return File(pdfBytes, "application/pdf", $"PaymentReceipt_HMS{id:D5}.pdf");
        }
    }
}
