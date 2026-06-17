using System;
using System.Threading.Tasks;

namespace HearMeStay.Services.Interfaces
{
    public interface IPdfExportService
    {
        Task<byte[]> GenerateBookingConfirmationPdfAsync(int bookingId);
        Task<byte[]> GeneratePaymentReceiptPdfAsync(int bookingId);
        Task<byte[]> GenerateGuestInsightPdfAsync(int bookingId);
        Task<byte[]> GenerateServiceFeeReportPdfAsync(DateTime startDate, DateTime endDate, string status, int? accommodationId);
        Task<byte[]> GenerateMarketingReportPdfAsync(DateTime startDate, DateTime endDate);
        Task<byte[]> GenerateBookingOperationLogPdfAsync(int bookingId);
    }
}
