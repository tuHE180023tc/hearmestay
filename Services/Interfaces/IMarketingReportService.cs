using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HearMeStay.ViewModels;

namespace HearMeStay.Services.Interfaces
{
    public interface IMarketingReportService
    {
        Task<MarketingOverviewViewModel> GetMarketingOverviewAsync(DateTime? startDate, DateTime? endDate, string? source, string? bookingStatus, int? accommodationId);
        Task<List<ConversionFunnelViewModel>> GetConversionFunnelAsync(DateTime? startDate, DateTime? endDate, string? source, int? accommodationId);
        Task<List<TrafficSourceReportViewModel>> GetTrafficSourceReportAsync(DateTime? startDate, DateTime? endDate);
        Task<List<TopAccommodationMarketingViewModel>> GetTopAccommodationsAsync(DateTime? startDate, DateTime? endDate);
        Task<AiPersonalizationReportViewModel> GetAiPersonalizationReportAsync(DateTime? startDate, DateTime? endDate);
        double CalculateConversionRate(int numerator, int denominator);
    }
}
