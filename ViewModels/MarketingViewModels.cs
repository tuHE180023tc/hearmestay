using System;
using System.Collections.Generic;

namespace HearMeStay.ViewModels
{
    public class MarketingReportFilterViewModel
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public string? Source { get; set; }
        public string? BookingStatus { get; set; }
        public int? AccommodationId { get; set; }
    }

    public class MarketingOverviewViewModel
    {
        public int WebsiteVisitors { get; set; }
        public int NewUsers { get; set; }
        public int BookingRequests { get; set; }
        public int HotelConfirmedBookings { get; set; }
        public int PaidBookings { get; set; }
        public int ConfirmedBookings { get; set; }
        public int RejectedBookings { get; set; }
        public double BookingConversionRate { get; set; }
        public double PaymentConversionRate { get; set; }
        public decimal TotalBookingValue { get; set; }
        public decimal ServiceFeeRevenue { get; set; }
        public double ReturnCustomerRate { get; set; }
        public int PreferenceProfileUsageCount { get; set; }
    }

    public class TrafficSourceReportViewModel
    {
        public string Source { get; set; } = string.Empty;
        public int Visitors { get; set; }
        public int NewUsers { get; set; }
        public int BookingRequests { get; set; }
        public int ConfirmedBookings { get; set; }
        public decimal ServiceFeeRevenue { get; set; }
        public double ConversionRate { get; set; }
    }

    public class ConversionFunnelViewModel
    {
        public string StepName { get; set; } = string.Empty;
        public int Count { get; set; }
        public double Percentage { get; set; }
    }

    public class TopAccommodationMarketingViewModel
    {
        public int AccommodationId { get; set; }
        public string AccommodationName { get; set; } = string.Empty;
        public int ViewCount { get; set; }
        public int BookingRequests { get; set; }
        public int ConfirmedBookings { get; set; }
        public decimal ServiceFeeRevenue { get; set; }
        public double ConversionRate { get; set; }
    }

    public class AiPersonalizationReportViewModel
    {
        public int TotalPreferenceForms { get; set; }
        public int TotalAiTags { get; set; }
        public Dictionary<string, int> TopTags { get; set; } = new Dictionary<string, int>();
        public int SavedPreferenceProfiles { get; set; }
        public int ReusedPreferenceProfiles { get; set; }
        public double BookingsWithSpecialRequestRate { get; set; }
    }
}
