using System;
using System.ComponentModel.DataAnnotations;

namespace HearMeStay.Models
{
    public class WebsiteVisitLog
    {
        public int Id { get; set; }
        
        [StringLength(100)]
        public string? SessionId { get; set; }
        
        [StringLength(450)]
        public string? UserId { get; set; }
        
        [StringLength(500)]
        public string? PageUrl { get; set; }
        
        [StringLength(500)]
        public string? ReferrerUrl { get; set; }
        
        [StringLength(100)]
        public string Source { get; set; } = "Direct";
        
        [StringLength(100)]
        public string? Medium { get; set; }
        
        [StringLength(100)]
        public string? Campaign { get; set; }
        
        [StringLength(100)]
        public string? DeviceType { get; set; }
        
        [StringLength(50)]
        public string? IpAddress { get; set; }
        
        public string? UserAgent { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
