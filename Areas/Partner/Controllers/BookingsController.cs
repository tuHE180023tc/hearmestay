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
        private readonly IEmailService _emailService;
        private readonly IBookingOperationLogService _operationLogService;
        private readonly IPdfExportService _pdfService;

        public BookingsController(ApplicationDbContext context, UserManager<ApplicationUser> userManager, IBookingService bookingService, IEmailService emailService, IBookingOperationLogService operationLogService, IPdfExportService pdfService)
        {
            _context = context;
            _userManager = userManager;
            _bookingService = bookingService;
            _emailService = emailService;
            _operationLogService = operationLogService;
            _pdfService = pdfService;
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
                .Include(b => b.OperationLogs.OrderByDescending(l => l.CreatedAt))
                .FirstOrDefaultAsync(b => b.Id == id && b.Accommodation.OwnerId == userId);
            if (booking == null) return NotFound();

            if (booking.BookingStatus == BookingStatus.Pending)
            {
                await _operationLogService.LogStatusChangeAsync(booking.Id, userId, "HotelPartner", booking.BookingStatus, booking.BookingStatus, HearMeStay.Models.Enums.BookingOperationActionType.PartnerViewedBooking, "Nơi lưu trú đã xem yêu cầu đặt phòng.");
            }

            return View(booking);
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Confirm(int id, HearMeStay.Models.Enums.SpecialRequestStatus specialRequestStatus, string? partnerSpecialRequestNote, decimal extraFee = 0)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.Include(b => b.Accommodation).Include(b => b.GuestPreference).Include(b => b.User).FirstOrDefaultAsync(b => b.Id == id && b.Accommodation.OwnerId == userId);
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

            await _operationLogService.LogStatusChangeAsync(id, userId, "HotelPartner", BookingStatus.Pending, BookingStatus.PaymentPending, HearMeStay.Models.Enums.BookingOperationActionType.PartnerConfirmedBooking, "Nơi lưu trú xác nhận còn phòng và có thể tiếp nhận booking. " + partnerSpecialRequestNote, "Chờ khách thanh toán.");

            // Gửi email cho khách hàng
            var targetEmail = !string.IsNullOrEmpty(booking.GuestEmail) ? booking.GuestEmail : booking.User?.Email;
            if (!string.IsNullOrEmpty(targetEmail))
            {
                var paymentLink = Url.Action("Details", "Bookings", new { id = booking.Id, area = "" }, Request.Scheme);
                var subject = $"Xác nhận đặt phòng tại {booking.Accommodation.Name} - Vui lòng thanh toán";
                var body = $@"
                    <h3>Xin chào {booking.GuestFullName},</h3>
                    <p>Đặt phòng của bạn tại <strong>{booking.Accommodation.Name}</strong> đã được xác nhận!</p>
                    <ul>
                        <li><strong>Ngày nhận phòng:</strong> {booking.CheckInDate:dd/MM/yyyy}</li>
                        <li><strong>Ngày trả phòng:</strong> {booking.CheckOutDate:dd/MM/yyyy}</li>
                        <li><strong>Số lượng:</strong> {booking.NumberOfGuests} khách, {booking.NumberOfRooms} phòng</li>
                    </ul>
                    <p>Phản hồi từ nơi lưu trú: {(string.IsNullOrEmpty(partnerSpecialRequestNote) ? "Không có" : partnerSpecialRequestNote)}</p>
                    <p>Để hoàn tất quá trình đặt phòng, vui lòng truy cập đường dẫn sau để xem thông tin chi tiết và thanh toán:</p>
                    <p><a href='{paymentLink}' style='padding: 10px 15px; background-color: #0d6efd; color: white; text-decoration: none; border-radius: 5px;'>Xem chi tiết & Thanh toán</a></p>
                    <p>Cảm ơn bạn đã tin tưởng HearMeStay!</p>
                ";
                await _emailService.SendEmailAsync(targetEmail, subject, body);
            }

            TempData["Success"] = "Booking đã được xác nhận và email thông báo đã được gửi cho khách hàng.";
            return RedirectToAction("Index");
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string? partnerNote)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.Include(b => b.Accommodation).FirstOrDefaultAsync(b => b.Id == id && b.Accommodation.OwnerId == userId);
            if (booking == null) return NotFound();
            await _bookingService.RejectBookingAsync(id, partnerNote);
            
            await _operationLogService.LogStatusChangeAsync(id, userId, "HotelPartner", BookingStatus.Pending, BookingStatus.Rejected, HearMeStay.Models.Enums.BookingOperationActionType.PartnerRejectedBooking, partnerNote, "Thông báo cho khách hoặc để khách chọn nơi lưu trú khác.");
            
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
            
            await _operationLogService.LogStatusChangeAsync(id, userId, "HotelPartner", BookingStatus.PaymentVerificationPending, BookingStatus.Confirmed, HearMeStay.Models.Enums.BookingOperationActionType.PaymentVerified, "Thanh toán hợp lệ. Booking đã được xác nhận.");
            
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
            
            await _operationLogService.LogStatusChangeAsync(id, userId, "HotelPartner", BookingStatus.PaymentVerificationPending, BookingStatus.PaymentPending, HearMeStay.Models.Enums.BookingOperationActionType.PaymentRejected, reason, "Khách cần kiểm tra lại thông tin thanh toán.");
            
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
            
            await _operationLogService.LogStatusChangeAsync(id, userId, "HotelPartner", BookingStatus.Confirmed, BookingStatus.Completed, HearMeStay.Models.Enums.BookingOperationActionType.BookingCompleted, "Khách đã hoàn tất lưu trú.");
            
            TempData["Success"] = "Lưu trú đã hoàn tất.";
            return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ExportBookingConfirmationPdf(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.Include(b => b.Accommodation).FirstOrDefaultAsync(b => b.Id == id && b.Accommodation.OwnerId == userId);
            if (booking == null) return NotFound();
            var pdfBytes = await _pdfService.GenerateBookingConfirmationPdfAsync(id);
            if (pdfBytes == null) return NotFound("Lỗi tạo PDF.");
            return File(pdfBytes, "application/pdf", $"BookingConfirmation_HMS{id:D5}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ExportGuestInsightPdf(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.Include(b => b.Accommodation).FirstOrDefaultAsync(b => b.Id == id && b.Accommodation.OwnerId == userId);
            if (booking == null) return NotFound();
            var pdfBytes = await _pdfService.GenerateGuestInsightPdfAsync(id);
            if (pdfBytes == null) return NotFound("Lỗi tạo PDF.");
            return File(pdfBytes, "application/pdf", $"GuestInsight_HMS{id:D5}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ExportOperationLogPdf(int id)
        {
            var userId = _userManager.GetUserId(User);
            var booking = await _context.Bookings.Include(b => b.Accommodation).FirstOrDefaultAsync(b => b.Id == id && b.Accommodation.OwnerId == userId);
            if (booking == null) return NotFound();
            var pdfBytes = await _pdfService.GenerateBookingOperationLogPdfAsync(id);
            if (pdfBytes == null) return NotFound("Lỗi tạo PDF.");
            return File(pdfBytes, "application/pdf", $"BookingOperationLog_HMS{id:D5}.pdf");
        }
    }
}
