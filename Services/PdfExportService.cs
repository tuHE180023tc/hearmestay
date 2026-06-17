using System;
using System.Linq;
using System.Threading.Tasks;
using HearMeStay.Data;
using HearMeStay.Models.Enums;
using HearMeStay.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace HearMeStay.Services
{
    public class PdfExportService : IPdfExportService
    {
        private readonly ApplicationDbContext _context;
        // Default font for Vietnamese support
        private const string StandardFont = "Arial";

        public PdfExportService(ApplicationDbContext context)
        {
            _context = context;
        }

        private void SetTheme(PageDescriptor page)
        {
            page.Margin(30);
            page.Size(PageSizes.A4);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(11).FontFamily(StandardFont).Fallback(x => x.FontFamily("Times New Roman")));
        }

        private void DrawHeader(IContainer container, string title)
        {
            container.Row(row =>
            {
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("HearMeStay").FontSize(24).SemiBold().FontColor(Colors.Teal.Darken2);
                    col.Item().Text("Trải nghiệm cá nhân hóa bằng AI").FontSize(12).FontColor(Colors.Grey.Medium);
                });
                row.ConstantItem(200).AlignRight().Text(title).FontSize(16).SemiBold();
            });
        }

        public async Task<byte[]> GenerateBookingConfirmationPdfAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Accommodation).ThenInclude(a => a.Owner)
                .Include(b => b.RoomType)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return null;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    SetTheme(page);
                    page.Header().Element(c => DrawHeader(c, "Phiếu Xác Nhận Đặt Phòng"));
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text($"Mã Booking: #{booking.Id:D5}").FontSize(14).SemiBold();
                        col.Item().Text($"Ngày tạo: {booking.CreatedAt:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Trạng thái: {TranslateBookingStatus(booking.BookingStatus)}").SemiBold().FontColor(Colors.Blue.Darken2);
                        
                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Text("Thông tin khách hàng:").SemiBold();
                        col.Item().Text($"- Tên khách hàng: {booking.GuestFullName}");
                        col.Item().Text($"- Email: {booking.GuestEmail}");
                        col.Item().Text($"- Số điện thoại: {booking.GuestPhone}");

                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Text("Thông tin lưu trú:").SemiBold();
                        col.Item().Text($"- Nơi lưu trú: {booking.Accommodation.Name}");
                        col.Item().Text($"- Địa chỉ: {booking.Accommodation.City}, {booking.Accommodation.Province}");
                        col.Item().Text($"- Loại phòng: {booking.RoomType.Name}");
                        col.Item().Text($"- Ngày nhận phòng: {booking.CheckInDate:dd/MM/yyyy}");
                        col.Item().Text($"- Ngày trả phòng: {booking.CheckOutDate:dd/MM/yyyy}");
                        var nights = (booking.CheckOutDate - booking.CheckInDate).Days;
                        col.Item().Text($"- Số đêm: {nights}");
                        col.Item().Text($"- Số khách: {booking.NumberOfGuests}");

                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        var origPrice = booking.TotalAmount / 1.15m;
                        var sFee = booking.TotalAmount - origPrice;

                        col.Item().Text("Chi tiết thanh toán:").SemiBold();
                        col.Item().Text($"- Giá gốc phòng: {origPrice:N0} VND");
                        col.Item().Text($"- Phí dịch vụ (15%): {sFee:N0} VND");
                        col.Item().Text($"- Tổng thanh toán: {booking.TotalAmount:N0} VND").SemiBold().FontColor(Colors.Red.Medium);
                        col.Item().Text($"- Trạng thái thanh toán: {TranslatePaymentStatus(booking.PaymentStatus)}");

                        col.Item().PaddingTop(20).Text("Booking này đã được xác nhận trên hệ thống HearMeStay.").Italic().FontColor(Colors.Grey.Medium);
                    });
                    page.Footer().AlignCenter().Text(x => {
                        x.Span("HearMeStay - Trang ");
                        x.CurrentPageNumber();
                        x.Span(" / ");
                        x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GeneratePaymentReceiptPdfAsync(int bookingId)
        {
            var booking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Accommodation)
                .Include(b => b.RoomType)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null) return null;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    SetTheme(page);
                    page.Header().Element(c => DrawHeader(c, "Biên Nhận Thanh Toán"));
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text($"Mã Booking: #{booking.Id:D5}").FontSize(14).SemiBold();
                        col.Item().Text($"Tên khách hàng: {booking.GuestFullName}");
                        col.Item().Text($"Nơi lưu trú: {booking.Accommodation.Name} - {booking.RoomType.Name}");
                        col.Item().Text($"Ngày thanh toán: {(booking.PaymentVerifiedAt ?? booking.CreatedAt):dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Phương thức: QR Chuyển khoản");

                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        var origPrice = booking.TotalAmount / 1.15m;
                        var sFee = booking.TotalAmount - origPrice;

                        col.Item().Text("Chi tiết khoản thu:").SemiBold();
                        col.Item().Text($"- Giá gốc phòng: {origPrice:N0} VND");
                        col.Item().Text($"- Phí dịch vụ (15%): {sFee:N0} VND");
                        col.Item().Text($"- Tổng tiền khách đã thanh toán: {booking.TotalAmount:N0} VND").SemiBold().FontColor(Colors.Red.Medium);

                        col.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        
                        col.Item().Text("Phân bổ doanh thu:").SemiBold();
                        col.Item().Text($"- Khách sạn nhận: {origPrice:N0} VND");
                        col.Item().Text($"- HearMeStay nhận (Phí dịch vụ): {sFee:N0} VND");
                        
                        col.Item().PaddingTop(10).Text($"Trạng thái xác minh: {TranslatePaymentStatus(booking.PaymentStatus)}").SemiBold();
                        col.Item().Text($"Nội dung chuyển khoản: HMS{booking.Id:D5}");

                        col.Item().PaddingTop(30).Text("Biên nhận này được tạo tự động từ hệ thống HearMeStay.").Italic().FontColor(Colors.Grey.Medium);
                    });
                    page.Footer().AlignCenter().Text(x => {
                        x.Span("HearMeStay - Trang "); x.CurrentPageNumber(); x.Span(" / "); x.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateGuestInsightPdfAsync(int bookingId)
        {
            var insight = await _context.GuestInsights
                .Include(g => g.GuestPreference).ThenInclude(p => p.Booking).ThenInclude(b => b.User)
                .Include(g => g.GuestPreference).ThenInclude(p => p.Booking).ThenInclude(b => b.Accommodation)
                .Include(g => g.GuestPreference).ThenInclude(p => p.Booking).ThenInclude(b => b.RoomType)
                .Include(g => g.Tags)
                .Include(g => g.Tasks)
                .FirstOrDefaultAsync(g => g.GuestPreference.BookingId == bookingId);

            if (insight == null) return null;
            var b = insight.GuestPreference.Booking;

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    SetTheme(page);
                    page.Header().Element(c => DrawHeader(c, "Phiếu Thông Tin Khách & AI Guest Insight"));
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text($"Mã Booking: #{b.Id:D5}").FontSize(14).SemiBold();
                        col.Item().Text($"Tên khách: {b.GuestFullName}");
                        col.Item().Text($"Ngày nhận/trả phòng: {b.CheckInDate:dd/MM/yyyy} - {b.CheckOutDate:dd/MM/yyyy} ({b.NumberOfGuests} khách)");
                        col.Item().Text($"Nơi lưu trú: {b.Accommodation.Name} - {b.RoomType.Name}");

                        if (!string.IsNullOrEmpty(insight.GuestPreference.RawText))
                        {
                            col.Item().PaddingTop(10).Text("Yêu cầu đặc biệt của khách:").SemiBold();
                            col.Item().PaddingBottom(10).Text(insight.GuestPreference.RawText).Italic();
                        }

                        col.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                        col.Item().Text("Danh sách AI Tags:").SemiBold().FontSize(12);
                        if (insight.Tags.Any())
                        {
                            foreach (var tag in insight.Tags)
                                col.Item().Text($"- {tag.TagName} (Mức độ: {TranslateSeverity(tag.Severity)})");
                        }
                        else
                        {
                            col.Item().Text("- Không có");
                        }

                        col.Item().PaddingTop(10).Text("Danh sách AI Tasks (Công việc đề xuất):").SemiBold().FontSize(12);
                        if (insight.Tasks.Any())
                        {
                            foreach (var task in insight.Tasks)
                                col.Item().Text($"- [{TranslateDepartment(task.Department)}] {task.Title}: {task.Description}");
                        }
                        else
                        {
                            col.Item().Text("- Không có");
                        }

                        col.Item().PaddingTop(30).Text("CẢNH BÁO: Thông tin này chỉ dùng để hỗ trợ chuẩn bị trải nghiệm lưu trú cho khách, không được chia sẻ ra bên ngoài.")
                            .SemiBold().FontColor(Colors.Red.Medium).Italic();
                    });
                    page.Footer().AlignCenter().Text(x => { x.Span("HearMeStay - Trang "); x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateServiceFeeReportPdfAsync(DateTime startDate, DateTime endDate, string status, int? accommodationId)
        {
            var query = _context.Bookings.Include(b => b.User).Include(b => b.Accommodation).AsQueryable();

            query = query.Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate);

            if (!string.IsNullOrEmpty(status))
            {
                if (Enum.TryParse<PaymentStatus>(status, out var pStatus))
                    query = query.Where(b => b.PaymentStatus == pStatus);
            }

            if (accommodationId.HasValue && accommodationId > 0)
                query = query.Where(b => b.AccommodationId == accommodationId.Value);

            var list = await query.OrderBy(b => b.CreatedAt).ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    SetTheme(page);
                    page.Size(PageSizes.A4.Landscape());
                    page.Header().Element(c => DrawHeader(c, "Báo Cáo Doanh Thu Phí Dịch Vụ"));
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text($"Khoảng thời gian: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}");
                        col.Item().Text($"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}");
                        col.Item().Text($"Tổng số booking: {list.Count}");
                        col.Item().Text($"Số booking đã thanh toán: {list.Count(b => b.PaymentStatus == PaymentStatus.Paid)}");
                        
                        var totalPaid = list.Sum(b => b.TotalAmount);
                        var totalOrig = totalPaid / 1.15m;
                        var totalFee = totalPaid - totalOrig;

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn(2);
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                                columns.RelativeColumn();
                            });
                            
                            table.Header(header =>
                            {
                                header.Cell().Text("Mã").SemiBold();
                                header.Cell().Text("Ngày tạo").SemiBold();
                                header.Cell().Text("Nơi lưu trú").SemiBold();
                                header.Cell().Text("Giá gốc").SemiBold();
                                header.Cell().Text("Phí DV 15%").SemiBold();
                                header.Cell().Text("Tổng").SemiBold();
                                header.Cell().Text("T.Thái TT").SemiBold();
                            });

                            foreach (var item in list)
                            {
                                var origP = item.TotalAmount / 1.15m;
                                var sFeeP = item.TotalAmount - origP;
                                table.Cell().Text($"#{item.Id:D5}");
                                table.Cell().Text($"{item.CreatedAt:dd/MM/yy}");
                                table.Cell().Text($"{item.Accommodation.Name}");
                                table.Cell().Text($"{origP:N0}");
                                table.Cell().Text($"{sFeeP:N0}");
                                table.Cell().Text($"{item.TotalAmount:N0}");
                                table.Cell().Text($"{TranslatePaymentStatus(item.PaymentStatus)}");
                            }

                            // Summary row
                            table.Cell().ColumnSpan(3).Text("TỔNG CỘNG").SemiBold().AlignRight();
                            table.Cell().Text($"{totalOrig:N0}").SemiBold();
                            table.Cell().Text($"{totalFee:N0}").SemiBold();
                            table.Cell().Text($"{totalPaid:N0}").SemiBold();
                            table.Cell().Text("");
                        });
                    });
                    page.Footer().AlignCenter().Text(x => { x.Span("HearMeStay - Trang "); x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateMarketingReportPdfAsync(DateTime startDate, DateTime endDate)
        {
            // Similar logic as MarketingReportService
            var visits = await _context.WebsiteVisitLogs.CountAsync(v => v.CreatedAt >= startDate && v.CreatedAt <= endDate);
            var newUsers = await _context.Users.CountAsync(u => u.CreatedAt >= startDate && u.CreatedAt <= endDate);
            
            var bookings = await _context.Bookings.Where(b => b.CreatedAt >= startDate && b.CreatedAt <= endDate).ToListAsync();
            var reqCount = bookings.Count;
            var confirmedCount = bookings.Count(b => b.BookingStatus == BookingStatus.Confirmed || b.BookingStatus == BookingStatus.Completed);
            var paidCount = bookings.Count(b => b.PaymentStatus == PaymentStatus.Paid);
            
            var totalPaidAmount = bookings.Where(b => b.PaymentStatus == PaymentStatus.Paid).Sum(b => b.TotalAmount);
            var totalFee = totalPaidAmount - (totalPaidAmount / 1.15m);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    SetTheme(page);
                    page.Header().Element(c => DrawHeader(c, "Báo Cáo Chỉ Số Marketing"));
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text($"Khoảng thời gian: {startDate:dd/MM/yyyy} - {endDate:dd/MM/yyyy}");
                        col.Item().PaddingTop(10).Text("1. Các chỉ số tổng quan").SemiBold().FontSize(14);
                        col.Item().Text($"- Lượt truy cập website: {visits}");
                        col.Item().Text($"- Người dùng mới: {newUsers}");
                        col.Item().Text($"- Tổng số Booking Request: {reqCount}");
                        col.Item().Text($"- Số Booking được xác nhận: {confirmedCount}");
                        col.Item().Text($"- Số Booking đã thanh toán: {paidCount}");
                        col.Item().Text($"- Doanh thu phí dịch vụ (15%): {totalFee:N0} VND").SemiBold();

                        // Additional AI Stats could go here...
                        col.Item().PaddingTop(20).Text("Báo cáo được tạo tự động bởi hệ thống Marketing Analytics - HearMeStay.").Italic().FontColor(Colors.Grey.Medium);
                    });
                    page.Footer().AlignCenter().Text(x => { x.Span("HearMeStay - Trang "); x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
                });
            });

            return document.GeneratePdf();
        }

        public async Task<byte[]> GenerateBookingOperationLogPdfAsync(int bookingId)
        {
            var booking = await _context.Bookings.Include(b => b.User).Include(b => b.Accommodation).FirstOrDefaultAsync(b => b.Id == bookingId);
            if (booking == null) return null;

            var logs = await _context.BookingOperationLogs
                .Where(l => l.BookingId == bookingId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    SetTheme(page);
                    page.Size(PageSizes.A4.Landscape());
                    page.Header().Element(c => DrawHeader(c, "Nhật Ký Xử Lý Booking"));
                    page.Content().PaddingVertical(20).Column(col =>
                    {
                        col.Spacing(10);
                        col.Item().Text($"Mã Booking: #{booking.Id:D5}").FontSize(14).SemiBold();
                        col.Item().Text($"Khách hàng: {booking.GuestFullName}");
                        col.Item().Text($"Nơi lưu trú: {booking.Accommodation.Name}");
                        col.Item().Text($"Trạng thái hiện tại: {TranslateBookingStatus(booking.BookingStatus)}");

                        col.Item().PaddingTop(10).Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(1);
                                columns.RelativeColumn(1.5f);
                                columns.RelativeColumn(2);
                                columns.RelativeColumn(2);
                            });
                            
                            table.Header(header =>
                            {
                                header.Cell().Text("Thời gian").SemiBold();
                                header.Cell().Text("Người thực hiện").SemiBold();
                                header.Cell().Text("Vai trò").SemiBold();
                                header.Cell().Text("Hành động").SemiBold();
                                header.Cell().Text("Trạng thái").SemiBold();
                                header.Cell().Text("Ghi chú").SemiBold();
                            });

                            foreach (var l in logs)
                            {
                                table.Cell().Text($"{l.CreatedAt:dd/MM/yy HH:mm}");
                                table.Cell().Text($"{(string.IsNullOrEmpty(l.ActorUserId) ? "System" : "User")}");
                                table.Cell().Text($"{l.ActorRole}");
                                table.Cell().Text($"{HearMeStay.Models.Enums.BookingOperationActionTypeHelper.GetDisplayName(l.ActionType)}");
                                table.Cell().Text($"{TranslateBookingStatus(l.OldStatus)} -> {TranslateBookingStatus(l.NewStatus)}");
                                table.Cell().Text($"{l.Note}");
                            }
                        });
                    });
                    page.Footer().AlignCenter().Text(x => { x.Span("HearMeStay - Trang "); x.CurrentPageNumber(); x.Span(" / "); x.TotalPages(); });
                });
            });

            return document.GeneratePdf();
        }

        private string TranslateBookingStatus(BookingStatus? status) => status switch
        {
            BookingStatus.Pending => "Chờ xác nhận",
            BookingStatus.PaymentPending => "Chờ thanh toán",
            BookingStatus.PaymentVerificationPending => "Chờ xác minh",
            BookingStatus.Confirmed => "Đã xác nhận",
            BookingStatus.Completed => "Hoàn thành",
            BookingStatus.Cancelled => "Đã hủy",
            BookingStatus.Rejected => "Từ chối",
            BookingStatus.PaymentExpired => "Hết hạn thanh toán",
            _ => "N/A"
        };

        private string TranslatePaymentStatus(PaymentStatus status) => status switch
        {
            PaymentStatus.Unpaid => "Chưa thanh toán",
            PaymentStatus.Paid => "Đã thanh toán",
            PaymentStatus.Refunded => "Đã hoàn tiền",
            _ => status.ToString()
        };

        private string TranslateSeverity(SeverityLevel l) => l switch { SeverityLevel.High => "Cao", SeverityLevel.Medium => "Vừa", SeverityLevel.Low => "Thấp", _ => "BT" };
        private string TranslateDepartment(Department d) => d switch { Department.Kitchen => "Bếp", Department.Reception => "Lễ tân", Department.Housekeeping => "Buồng phòng", Department.CustomerService => "CSKH", Department.Manager => "Quản lý", _ => d.ToString() };
    }
}
