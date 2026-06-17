using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HearMeStay.Services.Interfaces;
using HearMeStay.ViewModels;

namespace HearMeStay.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MarketingReportsController : Controller
    {
        private readonly IMarketingReportService _marketingService;
        private readonly IPdfExportService _pdfService;

        public MarketingReportsController(IMarketingReportService marketingService, IPdfExportService pdfService)
        {
            _marketingService = marketingService;
            _pdfService = pdfService;
        }

        public async Task<IActionResult> Index(MarketingReportFilterViewModel filter)
        {
            // Set default date range to last 30 days if not provided
            if (!filter.StartDate.HasValue && !filter.EndDate.HasValue)
            {
                filter.StartDate = DateTime.Now.AddDays(-30);
                filter.EndDate = DateTime.Now;
            }

            ViewBag.Filter = filter;

            ViewBag.Overview = await _marketingService.GetMarketingOverviewAsync(filter.StartDate, filter.EndDate, filter.Source, filter.BookingStatus, filter.AccommodationId);
            ViewBag.ConversionFunnel = await _marketingService.GetConversionFunnelAsync(filter.StartDate, filter.EndDate, filter.Source, filter.AccommodationId);
            ViewBag.TrafficSources = await _marketingService.GetTrafficSourceReportAsync(filter.StartDate, filter.EndDate);
            ViewBag.TopAccommodations = await _marketingService.GetTopAccommodationsAsync(filter.StartDate, filter.EndDate);
            ViewBag.AiReport = await _marketingService.GetAiPersonalizationReportAsync(filter.StartDate, filter.EndDate);

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ExportMarketingReportPdf(DateTime? startDate, DateTime? endDate)
        {
            var sDate = startDate ?? DateTime.Now.AddDays(-30);
            var eDate = endDate ?? DateTime.Now;
            var pdfBytes = await _pdfService.GenerateMarketingReportPdfAsync(sDate, eDate);
            if (pdfBytes == null) return NotFound("Không thể tạo báo cáo.");
            return File(pdfBytes, "application/pdf", $"MarketingReport_{DateTime.Now:yyyy-MM}.pdf");
        }
    }
}
