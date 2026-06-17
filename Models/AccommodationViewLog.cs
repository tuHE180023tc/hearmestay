using System;
using System.ComponentModel.DataAnnotations;

namespace HearMeStay.Models
{
    public class AccommodationViewLog
    {
        public int Id { get; set; }
        
        public int AccommodationId { get; set; }
        public virtual Accommodation Accommodation { get; set; } = null!;
        
        [StringLength(450)]
        public string? UserId { get; set; }
        
        [StringLength(100)]
        public string? SessionId { get; set; }
        
        [StringLength(100)]
        public string Source { get; set; } = "Direct";
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
