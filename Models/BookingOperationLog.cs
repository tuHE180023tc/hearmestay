using System;
using System.ComponentModel.DataAnnotations;
using HearMeStay.Models.Enums;

namespace HearMeStay.Models
{
    public class BookingOperationLog
    {
        public int Id { get; set; }

        public int BookingId { get; set; }
        public virtual Booking Booking { get; set; } = null!;

        [StringLength(450)]
        public string ActorUserId { get; set; } = string.Empty;

        [StringLength(50)]
        public string ActorRole { get; set; } = string.Empty;

        public BookingOperationActionType ActionType { get; set; }

        public BookingStatus? OldStatus { get; set; }
        public BookingStatus? NewStatus { get; set; }

        public string? Note { get; set; }
        public string? InternalNote { get; set; }
        
        [StringLength(50)]
        public string? ContactMethod { get; set; }
        
        [StringLength(100)]
        public string? ContactPerson { get; set; }
        
        public string? ContactResult { get; set; }
        
        public string? NextAction { get; set; }
        public DateTime? NextActionDueAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
