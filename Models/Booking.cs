using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using HearMeStay.Models.Enums;

namespace HearMeStay.Models
{
    /// <summary>
    /// Booking entity. Uses request-to-book model in MVP.
    /// Business rules:
    /// - New booking: Status=Pending, PaymentMethod=PayAtHotel, PaymentStatus=Unpaid
    /// - TotalAmount = PricePerNight * nights * NumberOfRooms
    /// - CommissionAmount = TotalAmount * CommissionRate
    /// </summary>
    public class Booking
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(20)]
        [Display(Name = "Mã đặt phòng")]
        public string BookingCode { get; set; } = string.Empty;

        [Required]
        public string UserId { get; set; } = string.Empty;

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; } = null!;

        [Required]
        public int AccommodationId { get; set; }

        [ForeignKey("AccommodationId")]
        public virtual Accommodation Accommodation { get; set; } = null!;

        [Required]
        public int RoomTypeId { get; set; }

        [ForeignKey("RoomTypeId")]
        public virtual RoomType RoomType { get; set; } = null!;

        [Required(ErrorMessage = "Vui lòng nhập họ tên khách.")]
        [StringLength(100)]
        [Display(Name = "Họ tên khách")]
        public string GuestFullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email khách.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        [StringLength(100)]
        [Display(Name = "Email khách")]
        public string GuestEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại khách.")]
        [Phone]
        [StringLength(20)]
        [Display(Name = "Số điện thoại khách")]
        public string GuestPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn ngày nhận phòng.")]
        [Display(Name = "Ngày nhận phòng")]
        [DataType(DataType.Date)]
        public DateTime CheckInDate { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn ngày trả phòng.")]
        [Display(Name = "Ngày trả phòng")]
        [DataType(DataType.Date)]
        public DateTime CheckOutDate { get; set; }

        [Required]
        [Range(1, 50, ErrorMessage = "Số khách phải từ 1 đến 50.")]
        [Display(Name = "Số khách")]
        public int NumberOfGuests { get; set; }

        [Required]
        [Range(1, 20, ErrorMessage = "Số phòng phải từ 1 đến 20.")]
        [Display(Name = "Số phòng")]
        public int NumberOfRooms { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Tổng tiền")]
        public decimal TotalAmount { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        [Display(Name = "Tỷ lệ hoa hồng")]
        public decimal CommissionRate { get; set; } = 0.08m;

        [Column(TypeName = "decimal(18,2)")]
        [Display(Name = "Hoa hồng")]
        public decimal CommissionAmount { get; set; }

        [Required]
        [Display(Name = "Hình thức thanh toán")]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.PayAtHotel;

        [Required]
        [Display(Name = "Trạng thái thanh toán")]
        public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;

        [Required]
        [Display(Name = "Trạng thái đặt phòng")]
        public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;

        [Display(Name = "Ghi chú của khách")]
        public string? GuestNote { get; set; }

        [Display(Name = "Phản hồi từ nơi lưu trú")]
        public string? PartnerResponseNote { get; set; }

        [Display(Name = "Ngày tạo")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Display(Name = "Ngày xác nhận")]
        public DateTime? ConfirmedAt { get; set; }

        [Display(Name = "Ngày hủy")]
        public DateTime? CancelledAt { get; set; }

        [Display(Name = "Mã QR thanh toán")]
        public string? PaymentQrImageUrl { get; set; }

        [Display(Name = "Ảnh chứng từ thanh toán")]
        public string? PaymentProofImageUrl { get; set; }

        [Display(Name = "Nội dung chuyển khoản")]
        public string? PaymentTransferContent { get; set; }

        [Display(Name = "Hạn chót thanh toán")]
        public DateTime? PaymentDeadline { get; set; }

        [Display(Name = "Thời gian xác minh thanh toán")]
        public DateTime? PaymentVerifiedAt { get; set; }

        [Display(Name = "Người xác minh thanh toán")]
        public string? PaymentVerifiedBy { get; set; }

        // Navigation properties
        public virtual GuestPreference? GuestPreference { get; set; }
        public virtual Review? Review { get; set; }
        public virtual CommissionTransaction? CommissionTransaction { get; set; }

        // Operation tracking
        public string? CurrentStep { get; set; }
        public string? NextAction { get; set; }
        public DateTime? NextActionDueAt { get; set; }
        public string? InternalNote { get; set; }
        public string? LastHandledByUserId { get; set; }
        public DateTime? LastHandledAt { get; set; }
        public virtual ICollection<BookingOperationLog> OperationLogs { get; set; } = new List<BookingOperationLog>();

        // Marketing Tracking
        public string? MarketingSource { get; set; }
        public string? MarketingMedium { get; set; }
        public string? MarketingCampaign { get; set; }
    }
}
