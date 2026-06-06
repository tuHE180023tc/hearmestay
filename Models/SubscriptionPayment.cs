using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HearMeStay.Models.Enums;

namespace HearMeStay.Models
{
    public class SubscriptionPayment
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int PartnerSubscriptionId { get; set; }

        [ForeignKey("PartnerSubscriptionId")]
        public virtual PartnerSubscription PartnerSubscription { get; set; } = null!;

        public decimal Amount { get; set; }

        public string? PaymentMethod { get; set; }

        public SubscriptionPaymentStatus PaymentStatus { get; set; } = SubscriptionPaymentStatus.Unpaid;

        public string? PaymentQrImageUrl { get; set; }

        public string? PaymentTransferContent { get; set; }

        public string? PaymentProofImageUrl { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public DateTime? PaidAt { get; set; }

        public DateTime? VerifiedAt { get; set; }

        public string? VerifiedBy { get; set; }
    }
}
