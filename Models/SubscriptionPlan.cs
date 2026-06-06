using System.ComponentModel.DataAnnotations;

namespace HearMeStay.Models
{
    public class SubscriptionPlan
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public decimal PricePerMonth { get; set; }

        public double CommissionRate { get; set; }

        public string? Features { get; set; } // JSON or comma separated

        public bool IsActive { get; set; } = true;
    }
}
