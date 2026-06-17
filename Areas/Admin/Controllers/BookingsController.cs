using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HearMeStay.Data;

namespace HearMeStay.Areas.Admin.Controllers
{
    [Area("Admin")][Authorize(Roles = "Admin")]
    public class BookingsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HearMeStay.Services.Interfaces.IPdfExportService _pdfService;
        
        public BookingsController(ApplicationDbContext ctx, HearMeStay.Services.Interfaces.IPdfExportService pdfService) { 
            _context = ctx; 
            _pdfService = pdfService;
        }
        public async Task<IActionResult> Index() => View(await _context.Bookings.Include(b => b.User).Include(b => b.Accommodation).Include(b => b.RoomType).OrderByDescending(b => b.CreatedAt).ToListAsync());
        public async Task<IActionResult> Details(int id) => View(await _context.Bookings.Include(b => b.User).Include(b => b.Accommodation).Include(b => b.RoomType).Include(b => b.GuestPreference).Include(b => b.OperationLogs.OrderByDescending(l => l.CreatedAt)).FirstOrDefaultAsync(b => b.Id == id));

        [HttpGet]
        public async Task<IActionResult> ExportBookingConfirmationPdf(int id)
        {
            var pdfBytes = await _pdfService.GenerateBookingConfirmationPdfAsync(id);
            if (pdfBytes == null) return NotFound("Booking không tồn tại hoặc lỗi tạo PDF.");
            return File(pdfBytes, "application/pdf", $"BookingConfirmation_HMS{id:D5}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ExportPaymentReceiptPdf(int id)
        {
            var pdfBytes = await _pdfService.GeneratePaymentReceiptPdfAsync(id);
            if (pdfBytes == null) return NotFound("Booking không tồn tại hoặc lỗi tạo PDF.");
            return File(pdfBytes, "application/pdf", $"PaymentReceipt_HMS{id:D5}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ExportGuestInsightPdf(int id)
        {
            var pdfBytes = await _pdfService.GenerateGuestInsightPdfAsync(id);
            if (pdfBytes == null) return NotFound("Guest Insight không tồn tại hoặc lỗi tạo PDF.");
            return File(pdfBytes, "application/pdf", $"GuestInsight_HMS{id:D5}.pdf");
        }

        [HttpGet]
        public async Task<IActionResult> ExportOperationLogPdf(int id)
        {
            var pdfBytes = await _pdfService.GenerateBookingOperationLogPdfAsync(id);
            if (pdfBytes == null) return NotFound("Log không tồn tại hoặc lỗi tạo PDF.");
            return File(pdfBytes, "application/pdf", $"BookingOperationLog_HMS{id:D5}.pdf");
        }
    }
}
