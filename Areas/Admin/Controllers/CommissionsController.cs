using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HearMeStay.Data;
using HearMeStay.Models.Enums;

namespace HearMeStay.Areas.Admin.Controllers
{
    [Area("Admin")][Authorize(Roles = "Admin")]
    public class CommissionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly HearMeStay.Services.Interfaces.IPdfExportService _pdfService;
        
        public CommissionsController(ApplicationDbContext ctx, HearMeStay.Services.Interfaces.IPdfExportService pdfService) { 
            _context = ctx; 
            _pdfService = pdfService;
        }

        public async Task<IActionResult> Index() => View(await _context.CommissionTransactions.Include(c => c.Booking).ThenInclude(b => b.User).Include(c => c.Accommodation).OrderByDescending(c => c.CreatedAt).ToListAsync());

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkAsPaid(int id)
        {
            var c = await _context.CommissionTransactions.FindAsync(id);
            if (c != null) { c.Status = CommissionStatus.Paid; c.PaidAt = DateTime.Now; await _context.SaveChangesAsync(); }
            TempData["Success"] = "Đã đánh dấu thanh toán."; return RedirectToAction("Index");
        }

        [HttpGet]
        public async Task<IActionResult> ExportServiceFeeReportPdf()
        {
            // For MVP, just export the last 30 days or all if needed.
            var startDate = DateTime.MinValue; // Change this to be filterable if we add filters to Index later
            var endDate = DateTime.MaxValue;
            var pdfBytes = await _pdfService.GenerateServiceFeeReportPdfAsync(startDate, endDate, null, null);
            if (pdfBytes == null) return NotFound("Không thể tạo báo cáo.");
            return File(pdfBytes, "application/pdf", $"ServiceFeeReport_{DateTime.Now:yyyy-MM}.pdf");
        }
    }
}
