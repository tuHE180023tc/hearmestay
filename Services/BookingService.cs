using Microsoft.EntityFrameworkCore;
using HearMeStay.Data;
using HearMeStay.Models;
using HearMeStay.Models.Enums;
using HearMeStay.Services.Interfaces;

namespace HearMeStay.Services
{
    public class BookingService : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public BookingService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<Booking> CreateBookingAsync(Booking booking)
        {
            booking.BookingCode = GenerateBookingCode();
            booking.BookingStatus = BookingStatus.Pending;
            booking.PaymentMethod = PaymentMethod.PayAtHotel;
            booking.PaymentStatus = PaymentStatus.Unpaid;
            booking.CreatedAt = DateTime.Now;

            var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
            var roomType = await _context.RoomTypes.FindAsync(booking.RoomTypeId);
            if (roomType != null)
            {
                booking.TotalAmount = CalculateTotalAmount(roomType.PricePerNight, booking.CheckInDate, booking.CheckOutDate, booking.NumberOfRooms);
                booking.CommissionAmount = CalculateCommission(booking.TotalAmount, booking.CommissionRate);
            }

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Notify hotel partner
            var accommodation = await _context.Accommodations.FindAsync(booking.AccommodationId);
            if (accommodation != null)
            {
                await _notificationService.CreateNotificationAsync(
                    accommodation.OwnerId,
                    "Đặt phòng mới",
                    $"Bạn có đặt phòng mới #{booking.BookingCode} cần xác nhận.",
                    "BookingCreated");
            }

            return booking;
        }

        public async Task<Booking?> ConfirmBookingAsync(int bookingId, string? partnerNote = null, decimal extraFee = 0)
        {
            var booking = await _context.Bookings.Include(b => b.Accommodation).FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null || booking.BookingStatus != BookingStatus.Pending) return null;

            booking.BookingStatus = BookingStatus.PaymentPending;
            booking.PartnerResponseNote = partnerNote;
            
            // Add extra fee if provided
            if (extraFee > 0)
            {
                booking.TotalAmount += extraFee;
                // Optionally update commission if you want commission to apply to extra fees too
                booking.CommissionAmount = CalculateCommission(booking.TotalAmount, booking.CommissionRate);
            }

            // Mock QR and Transfer Content for Payment step
            booking.PaymentDeadline = DateTime.Now.AddHours(24);
            long amountInt = (long)booking.TotalAmount; // Ép kiểu số nguyên để VietQR nhận dạng đúng số tiền
            string encodedName = Uri.EscapeDataString("TO CHINH TU");
            booking.PaymentQrImageUrl = $"https://img.vietqr.io/image/tpbank-81655940116-compact2.jpg?amount={amountInt}&addInfo={booking.BookingCode}&accountName={encodedName}";
            booking.PaymentTransferContent = booking.BookingCode;

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                booking.UserId,
                "Yêu cầu đặt phòng đã được xác nhận",
                $"Nơi lưu trú đã xác nhận còn phòng cho #{booking.BookingCode}. Vui lòng thanh toán để hoàn tất.",
                "BookingPaymentPending");

            return booking;
        }

        public async Task<Booking?> SubmitPaymentProofAsync(int bookingId, string transferContent, string? proofImageUrl = null)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null || booking.BookingStatus != BookingStatus.PaymentPending) return null;

            booking.BookingStatus = BookingStatus.PaymentVerificationPending;
            // Optionally save the uploaded image url if provided (assuming it was uploaded elsewhere)
            if (!string.IsNullOrEmpty(proofImageUrl))
            {
                booking.PaymentProofImageUrl = proofImageUrl;
            }
            await _context.SaveChangesAsync();

            // Notify partner
            var accommodation = await _context.Accommodations.FindAsync(booking.AccommodationId);
            if (accommodation != null)
            {
                await _notificationService.CreateNotificationAsync(
                    accommodation.OwnerId,
                    "Khách đã thanh toán",
                    $"Khách hàng đã báo thanh toán cho booking #{booking.BookingCode}. Vui lòng kiểm tra và xác minh.",
                    "PaymentVerificationNeeded");
            }

            return booking;
        }

        public async Task<Booking?> VerifyPaymentAsync(int bookingId, string adminOrPartnerName)
        {
            var booking = await _context.Bookings.Include(b => b.Accommodation).FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null || booking.BookingStatus != BookingStatus.PaymentVerificationPending) return null;

            booking.BookingStatus = BookingStatus.Confirmed;
            booking.PaymentStatus = PaymentStatus.Paid;
            booking.ConfirmedAt = DateTime.Now;
            booking.PaymentVerifiedAt = DateTime.Now;
            booking.PaymentVerifiedBy = adminOrPartnerName;

            // Create commission transaction now that payment is verified
            var commission = new CommissionTransaction
            {
                BookingId = booking.Id,
                AccommodationId = booking.AccommodationId,
                TotalAmount = booking.TotalAmount,
                CommissionRate = booking.CommissionRate,
                CommissionAmount = booking.CommissionAmount,
                Status = CommissionStatus.Payable
            };
            _context.CommissionTransactions.Add(commission);
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                booking.UserId,
                "Thanh toán thành công",
                $"Thanh toán cho đặt phòng #{booking.BookingCode} đã được xác nhận. Chúc bạn có kỳ nghỉ vui vẻ!",
                "BookingConfirmed");

            return booking;
        }

        public async Task<Booking?> RejectPaymentAsync(int bookingId, string reason)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null || booking.BookingStatus != BookingStatus.PaymentVerificationPending) return null;

            // Revert back to PaymentPending so they can pay again
            booking.BookingStatus = BookingStatus.PaymentPending;
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                booking.UserId,
                "Chưa nhận được thanh toán",
                $"Nơi lưu trú báo cáo chưa nhận được khoản thanh toán cho đặt phòng #{booking.BookingCode}. Lý do: {reason}. Vui lòng kiểm tra lại hoặc liên hệ hỗ trợ.",
                "PaymentRejected");

            return booking;
        }

        public async Task<Booking?> RejectBookingAsync(int bookingId, string? partnerNote = null)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null || booking.BookingStatus != BookingStatus.Pending) return null;

            booking.BookingStatus = BookingStatus.Rejected;
            booking.PartnerResponseNote = partnerNote;
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                booking.UserId,
                "Đặt phòng bị từ chối",
                $"Đặt phòng #{booking.BookingCode} đã bị từ chối. Lý do: {partnerNote ?? "Không có lý do cụ thể."}",
                "BookingRejected");

            return booking;
        }

        public async Task<Booking?> CancelBookingAsync(int bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null) return null;
            if (booking.BookingStatus != BookingStatus.Pending && 
                booking.BookingStatus != BookingStatus.Confirmed && 
                booking.BookingStatus != BookingStatus.PaymentPending) return null;

            booking.BookingStatus = BookingStatus.Cancelled;
            booking.CancelledAt = DateTime.Now;
            await _context.SaveChangesAsync();
            return booking;
        }

        public async Task<Booking?> MarkCompletedAsync(int bookingId)
        {
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null || booking.BookingStatus != BookingStatus.Confirmed) return null;

            booking.BookingStatus = BookingStatus.Completed;
            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                booking.UserId,
                "Lưu trú hoàn tất",
                $"Cảm ơn bạn đã lưu trú! Hãy chia sẻ đánh giá cho đặt phòng #{booking.BookingCode}.",
                "BookingCompleted");

            return booking;
        }

        public decimal CalculateTotalAmount(decimal pricePerNight, DateTime checkIn, DateTime checkOut, int numberOfRooms)
        {
            var nights = (checkOut - checkIn).Days;
            if (nights <= 0) nights = 1;
            return pricePerNight * nights * numberOfRooms;
        }

        public decimal CalculateCommission(decimal totalAmount, decimal commissionRate = 0.08m)
        {
            return totalAmount * commissionRate;
        }

        public string GenerateBookingCode()
        {
            return $"HMS{DateTime.Now:yyyyMMdd}{Random.Shared.Next(10000, 99999)}";
        }
    }
}
