namespace HearMeStay.Models.Enums
{
    public static class BookingOperationActionTypeHelper
    {
        public static string GetDisplayName(BookingOperationActionType actionType)
        {
            return actionType switch
            {
                BookingOperationActionType.BookingCreated => "Tạo yêu cầu đặt phòng",
                BookingOperationActionType.PreferenceSubmitted => "Khách đã gửi nhu cầu cá nhân",
                BookingOperationActionType.AiAnalyzed => "AI đã phân tích nhu cầu khách",
                BookingOperationActionType.SentToHotelPartner => "Đã gửi yêu cầu đến nơi lưu trú",
                BookingOperationActionType.PartnerViewedBooking => "Nơi lưu trú đã xem booking",
                BookingOperationActionType.PartnerConfirmedBooking => "Nơi lưu trú đã xác nhận booking",
                BookingOperationActionType.PartnerRejectedBooking => "Nơi lưu trú đã từ chối booking",
                BookingOperationActionType.PaymentOpened => "Khách đã mở trang thanh toán",
                BookingOperationActionType.PaymentSubmitted => "Khách đã báo thanh toán",
                BookingOperationActionType.PaymentProofUploaded => "Khách đã tải lên biên lai",
                BookingOperationActionType.PaymentVerified => "Thanh toán đã được xác minh",
                BookingOperationActionType.PaymentRejected => "Thanh toán bị từ chối",
                BookingOperationActionType.BookingConfirmed => "Booking đã được xác nhận",
                BookingOperationActionType.BookingCancelled => "Booking đã bị hủy",
                BookingOperationActionType.BookingCompleted => "Booking đã hoàn thành",
                BookingOperationActionType.NoteAdded => "Đã thêm ghi chú",
                BookingOperationActionType.InternalNoteAdded => "Đã thêm ghi chú nội bộ",
                BookingOperationActionType.NextActionUpdated => "Đã cập nhật việc cần làm tiếp theo",
                BookingOperationActionType.Reassigned => "Đã chuyển người phụ trách",
                _ => actionType.ToString()
            };
        }
    }
}
