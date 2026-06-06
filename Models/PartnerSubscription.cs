using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HearMeStay.Models.Enums;

namespace HearMeStay.Models
{
    public class PartnerSubscription
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string PartnerId { get; set; } = string.Empty;

        [ForeignKey("PartnerId")]
        public virtual ApplicationUser Partner { get; set; } = null!;

        [Required]
        public int SubscriptionPlanId { get; set; }

        [ForeignKey("SubscriptionPlanId")]
        public virtual SubscriptionPlan SubscriptionPlan { get; set; } = null!;

        public DateTime StartDate { get; set; }

        public DateTime EndDate { get; set; }

        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Free;

        public bool IsAutoRenew { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
