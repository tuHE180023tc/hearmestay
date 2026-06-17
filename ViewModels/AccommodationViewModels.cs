using HearMeStay.Models.Enums;

namespace HearMeStay.ViewModels
{
    public class AccommodationSearchViewModel
    {
        public string? City { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public int? Guests { get; set; }
        public decimal? PriceMin { get; set; }
        public decimal? PriceMax { get; set; }
        public AccommodationType? AccommodationType { get; set; }
        public bool? IsQuietRoom { get; set; }
        public bool? SupportsVeganMeal { get; set; }
        public bool? SupportsAllergyRequest { get; set; }
        public bool? NoStrongScentAvailable { get; set; }
        public bool? HasPrivateBathroom { get; set; }
        public List<AccommodationCardViewModel> Results { get; set; } = new();
    }

    public class AccommodationCardViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Province { get; set; } = string.Empty;
        public AccommodationType AccommodationType { get; set; }
        public int? StarRating { get; set; }
        public string? MainImageUrl { get; set; }
        public decimal MinPrice { get; set; }
        public double AverageRating { get; set; }
        public int ReviewCount { get; set; }
        public bool HasQuietRoom { get; set; }
        public bool HasVeganMeal { get; set; }
        public bool HasAllergySupport { get; set; }
        public bool HasPrivateBathroom { get; set; }
        public bool NoStrongScentAvailable { get; set; }
    }
}
