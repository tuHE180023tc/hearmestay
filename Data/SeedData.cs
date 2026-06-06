using Microsoft.AspNetCore.Identity;
using HearMeStay.Models;
using HearMeStay.Models.Enums;

namespace HearMeStay.Data
{
    public static class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var context = serviceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed Roles
            string[] roles = { "Admin", "HotelPartner", "Traveler" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                    await roleManager.CreateAsync(new IdentityRole(role));
            }

            // Seed Admin
            var adminEmail = "admin@hearmestay.vn";
            if (await userManager.FindByEmailAsync(adminEmail) == null)
            {
                var admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = "Admin HearMeStay",
                    EmailConfirmed = true,
                    IsActive = true,
                    CreatedAt = DateTime.Now
                };
                await userManager.CreateAsync(admin, "Admin@123");
                await userManager.AddToRoleAsync(admin, "Admin");
            }

            // Seed HotelPartners
            var partner1Email = "partner1@hearmestay.vn";
            if (await userManager.FindByEmailAsync(partner1Email) == null)
            {
                var p1 = new ApplicationUser
                {
                    UserName = partner1Email, Email = partner1Email,
                    FullName = "Nguyễn Văn Hùng", EmailConfirmed = true,
                    IsActive = true, CreatedAt = DateTime.Now
                };
                await userManager.CreateAsync(p1, "Partner@123");
                await userManager.AddToRoleAsync(p1, "HotelPartner");
            }

            var partner2Email = "partner2@hearmestay.vn";
            if (await userManager.FindByEmailAsync(partner2Email) == null)
            {
                var p2 = new ApplicationUser
                {
                    UserName = partner2Email, Email = partner2Email,
                    FullName = "Trần Thị Mai", EmailConfirmed = true,
                    IsActive = true, CreatedAt = DateTime.Now
                };
                await userManager.CreateAsync(p2, "Partner@123");
                await userManager.AddToRoleAsync(p2, "HotelPartner");
            }

            // Seed Travelers
            var traveler1Email = "traveler1@gmail.com";
            if (await userManager.FindByEmailAsync(traveler1Email) == null)
            {
                var t1 = new ApplicationUser
                {
                    UserName = traveler1Email, Email = traveler1Email,
                    FullName = "Lê Minh Tuấn", EmailConfirmed = true,
                    IsActive = true, CreatedAt = DateTime.Now
                };
                await userManager.CreateAsync(t1, "Traveler@123");
                await userManager.AddToRoleAsync(t1, "Traveler");
            }

            var traveler2Email = "traveler2@gmail.com";
            if (await userManager.FindByEmailAsync(traveler2Email) == null)
            {
                var t2 = new ApplicationUser
                {
                    UserName = traveler2Email, Email = traveler2Email,
                    FullName = "Phạm Thị Lan", EmailConfirmed = true,
                    IsActive = true, CreatedAt = DateTime.Now
                };
                await userManager.CreateAsync(t2, "Traveler@123");
                await userManager.AddToRoleAsync(t2, "Traveler");
            }

            // Seed Subscription Plans
            if (!context.SubscriptionPlans.Any())
            {
                var plans = new List<SubscriptionPlan>
                {
                    new SubscriptionPlan { Name = "Free Listing", PricePerMonth = 0, CommissionRate = 12.0, Features = "Đăng tối đa 1 nơi lưu trú,Phân tích AI cơ bản,Nhận booking cơ bản" },
                    new SubscriptionPlan { Name = "Professional", PricePerMonth = 1499000, CommissionRate = 8.0, Features = "Không giới hạn nơi lưu trú,Phân tích AI chuyên sâu,Xuất báo cáo doanh thu chi tiết,Hỗ trợ ưu tiên 24/7" },
                    new SubscriptionPlan { Name = "Premium", PricePerMonth = 4999000, CommissionRate = 3.0, Features = "Tất cả của gói Pro,Hỗ trợ marketing đa kênh,Tùy chỉnh luồng booking,Tích hợp API hệ thống PMS riêng" }
                };
                context.SubscriptionPlans.AddRange(plans);
                await context.SaveChangesAsync();
            }

            // Seed Amenities
            if (!context.Amenities.Any())
            {
                var amenities = new List<Amenity>
                {
                    new() { Name = "Wifi miễn phí", IconClass = "bi-wifi" },
                    new() { Name = "Bữa sáng", IconClass = "bi-cup-hot" },
                    new() { Name = "Hồ bơi", IconClass = "bi-water" },
                    new() { Name = "Đưa đón sân bay", IconClass = "bi-airplane" },
                    new() { Name = "BBQ", IconClass = "bi-fire" },
                    new() { Name = "Spa", IconClass = "bi-heart" },
                    new() { Name = "Phòng yên tĩnh", IconClass = "bi-volume-mute" },
                    new() { Name = "Hỗ trợ ăn chay", IconClass = "bi-leaf" },
                    new() { Name = "Thân thiện với khách dị ứng", IconClass = "bi-shield-check" },
                    new() { Name = "Bãi đỗ xe", IconClass = "bi-p-square" },
                    new() { Name = "Điều hòa", IconClass = "bi-snow" },
                    new() { Name = "Giặt ủi", IconClass = "bi-basket" }
                };
                context.Amenities.AddRange(amenities);
                await context.SaveChangesAsync();
            }

            // Seed Accommodations (only if none exist)
            if (!context.Accommodations.Any())
            {
                var p1 = await userManager.FindByEmailAsync(partner1Email);
                var p2 = await userManager.FindByEmailAsync(partner2Email);
                if (p1 == null || p2 == null) return;

                var acc1 = new Accommodation
                {
                    OwnerId = p1.Id, Name = "Villa Biển Xanh Hạ Long",
                    Slug = "villa-bien-xanh-ha-long",
                    Description = "Villa sang trọng view biển tại Hạ Long, phù hợp cho gia đình và cặp đôi muốn tận hưởng không gian riêng tư với bể bơi vô cực.",
                    Address = "123 Đường Bao Biển", City = "Hạ Long", Province = "Quảng Ninh",
                    AccommodationType = AccommodationType.Villa, StarRating = 5,
                    Phone = "0912345678", Email = "villa.bienxanh@email.com",
                    CheckInTime = new TimeSpan(14, 0, 0), CheckOutTime = new TimeSpan(12, 0, 0),
                    CancellationPolicy = "Hủy miễn phí trước 48 giờ",
                    Status = AccommodationStatus.Approved, IsActive = true
                };

                var acc2 = new Accommodation
                {
                    OwnerId = p1.Id, Name = "Homestay Núi Mây Tam Đảo",
                    Slug = "homestay-nui-may-tam-dao",
                    Description = "Homestay ấm cúng giữa rừng thông Tam Đảo, không gian yên tĩnh lý tưởng cho người muốn nghỉ ngơi và thư giãn.",
                    Address = "45 Đường Lên Tam Đảo", City = "Tam Đảo", Province = "Vĩnh Phúc",
                    AccommodationType = AccommodationType.Homestay,
                    Phone = "0923456789", Email = "nuimay.tamdao@email.com",
                    CheckInTime = new TimeSpan(14, 0, 0), CheckOutTime = new TimeSpan(11, 0, 0),
                    CancellationPolicy = "Hủy miễn phí trước 24 giờ",
                    Status = AccommodationStatus.Approved, IsActive = true
                };

                var acc3 = new Accommodation
                {
                    OwnerId = p2.Id, Name = "Khách Sạn Boutique Phố Cổ",
                    Slug = "khach-san-boutique-pho-co",
                    Description = "Khách sạn boutique phong cách Đông Dương giữa lòng phố cổ Hà Nội, kết hợp nét truyền thống và hiện đại.",
                    Address = "78 Hàng Bông", City = "Hà Nội", Province = "Hà Nội",
                    AccommodationType = AccommodationType.Hotel, StarRating = 4,
                    Phone = "0934567890", Email = "boutique.phoco@email.com",
                    CheckInTime = new TimeSpan(14, 0, 0), CheckOutTime = new TimeSpan(12, 0, 0),
                    CancellationPolicy = "Hủy miễn phí trước 24 giờ",
                    Status = AccommodationStatus.Approved, IsActive = true
                };

                context.Accommodations.AddRange(acc1, acc2, acc3);
                await context.SaveChangesAsync();

                // Seed AccommodationAmenities
                var allAmenities = context.Amenities.ToList();
                var aaList = new List<AccommodationAmenity>();
                foreach (var am in allAmenities.Take(8))
                    aaList.Add(new AccommodationAmenity { AccommodationId = acc1.Id, AmenityId = am.Id });
                foreach (var am in allAmenities.Take(6))
                    aaList.Add(new AccommodationAmenity { AccommodationId = acc2.Id, AmenityId = am.Id });
                foreach (var am in allAmenities.Take(9))
                    aaList.Add(new AccommodationAmenity { AccommodationId = acc3.Id, AmenityId = am.Id });
                context.AccommodationAmenities.AddRange(aaList);

                // Seed AccommodationImages (placeholders)
                context.AccommodationImages.AddRange(
                    new AccommodationImage { AccommodationId = acc1.Id, ImageUrl = "/images/placeholder-villa.svg", IsMain = true },
                    new AccommodationImage { AccommodationId = acc2.Id, ImageUrl = "/images/placeholder-homestay.svg", IsMain = true },
                    new AccommodationImage { AccommodationId = acc3.Id, ImageUrl = "/images/placeholder-hotel.svg", IsMain = true }
                );

                // Seed RoomTypes
                var room1 = new RoomType
                {
                    AccommodationId = acc1.Id, Name = "Villa Riêng Có Hồ Bơi",
                    Description = "Villa riêng với hồ bơi vô cực, view biển tuyệt đẹp.",
                    PricePerNight = 5500000, Capacity = 6, TotalRooms = 3, AvailableRooms = 2,
                    BedType = "King + 2 đơn", RoomSize = 120, HasPrivateBathroom = true,
                    IsQuietRoom = true, SupportsVeganMeal = true, SupportsAllergyRequest = true
                };
                var room2 = new RoomType
                {
                    AccommodationId = acc1.Id, Name = "Phòng Deluxe Hướng Biển",
                    Description = "Phòng rộng rãi, ban công hướng biển, nội thất gỗ tự nhiên.",
                    PricePerNight = 2800000, Capacity = 2, TotalRooms = 10, AvailableRooms = 7,
                    BedType = "King", RoomSize = 45, HasPrivateBathroom = true
                };
                var room3 = new RoomType
                {
                    AccommodationId = acc2.Id, Name = "Phòng Yên Tĩnh",
                    Description = "Phòng cách âm giữa rừng thông, lý tưởng để thư giãn.",
                    PricePerNight = 1200000, Capacity = 2, TotalRooms = 5, AvailableRooms = 4,
                    BedType = "Queen", RoomSize = 30, HasPrivateBathroom = true,
                    IsQuietRoom = true, NoStrongScentAvailable = true
                };
                var room4 = new RoomType
                {
                    AccommodationId = acc2.Id, Name = "Phòng Gia Đình",
                    Description = "Phòng rộng cho gia đình, có khu vực vui chơi cho trẻ em.",
                    PricePerNight = 1800000, Capacity = 4, TotalRooms = 3, AvailableRooms = 2,
                    BedType = "King + 2 đơn", RoomSize = 50, HasPrivateBathroom = true,
                    SupportsAllergyRequest = true
                };
                var room5 = new RoomType
                {
                    AccommodationId = acc3.Id, Name = "Phòng Couple Kỷ Niệm",
                    Description = "Phòng lãng mạn cho cặp đôi, trang trí đặc biệt theo yêu cầu.",
                    PricePerNight = 2200000, Capacity = 2, TotalRooms = 5, AvailableRooms = 3,
                    BedType = "King", RoomSize = 35, HasPrivateBathroom = true,
                    IsQuietRoom = true, SupportsVeganMeal = true
                };
                context.RoomTypes.AddRange(room1, room2, room3, room4, room5);

                // Seed AddOnServices
                context.AddOnServices.AddRange(
                    new AddOnService { AccommodationId = acc1.Id, Name = "Trang trí phòng kỷ niệm", Description = "Trang trí hoa, nến và rượu vang cho dịp đặc biệt.", Price = 800000, ServiceType = ServiceType.Decoration },
                    new AddOnService { AccommodationId = acc1.Id, Name = "Đưa đón sân bay", Description = "Xe riêng đưa đón sân bay Vân Đồn.", Price = 1200000, ServiceType = ServiceType.AirportPickup },
                    new AddOnService { AccommodationId = acc1.Id, Name = "Tiệc BBQ riêng", Description = "Tiệc BBQ hải sản tươi sống tại villa.", Price = 1500000, ServiceType = ServiceType.BBQ },
                    new AddOnService { AccommodationId = acc3.Id, Name = "Spa truyền thống", Description = "Liệu trình spa thảo mộc truyền thống.", Price = 600000, ServiceType = ServiceType.Spa },
                    new AddOnService { AccommodationId = acc3.Id, Name = "Tour phố cổ", Description = "Tour đi bộ khám phá phố cổ Hà Nội.", Price = 350000, ServiceType = ServiceType.Tour }
                );

                // Seed RoomImages (placeholders)
                await context.SaveChangesAsync();
                context.RoomImages.AddRange(
                    new RoomImage { RoomTypeId = room1.Id, ImageUrl = "/images/placeholder-room.svg", IsMain = true },
                    new RoomImage { RoomTypeId = room2.Id, ImageUrl = "/images/placeholder-room.svg", IsMain = true },
                    new RoomImage { RoomTypeId = room3.Id, ImageUrl = "/images/placeholder-room.svg", IsMain = true },
                    new RoomImage { RoomTypeId = room4.Id, ImageUrl = "/images/placeholder-room.svg", IsMain = true },
                    new RoomImage { RoomTypeId = room5.Id, ImageUrl = "/images/placeholder-room.svg", IsMain = true }
                );

                await context.SaveChangesAsync();

                // Seed sample bookings
                var t1 = await userManager.FindByEmailAsync(traveler1Email);
                var t2 = await userManager.FindByEmailAsync(traveler2Email);
                if (t1 != null && t2 != null)
                {
                    var booking1 = new Booking
                    {
                        BookingCode = "HMS2026010100001",
                        UserId = t1.Id, AccommodationId = acc1.Id, RoomTypeId = room1.Id,
                        GuestFullName = t1.FullName, GuestEmail = t1.Email!, GuestPhone = "0901234567",
                        CheckInDate = DateTime.Now.AddDays(7), CheckOutDate = DateTime.Now.AddDays(10),
                        NumberOfGuests = 2, NumberOfRooms = 1,
                        TotalAmount = 16500000, CommissionRate = 0.08m, CommissionAmount = 1320000,
                        BookingStatus = BookingStatus.Confirmed, ConfirmedAt = DateTime.Now.AddDays(-1)
                    };
                    var booking2 = new Booking
                    {
                        BookingCode = "HMS2026010100002",
                        UserId = t2.Id, AccommodationId = acc3.Id, RoomTypeId = room5.Id,
                        GuestFullName = t2.FullName, GuestEmail = t2.Email!, GuestPhone = "0907654321",
                        CheckInDate = DateTime.Now.AddDays(14), CheckOutDate = DateTime.Now.AddDays(16),
                        NumberOfGuests = 2, NumberOfRooms = 1,
                        TotalAmount = 4400000, CommissionRate = 0.08m, CommissionAmount = 352000,
                        BookingStatus = BookingStatus.Confirmed, ConfirmedAt = DateTime.Now.AddDays(-2)
                    };
                    var booking3 = new Booking
                    {
                        BookingCode = "HMS2026010100003",
                        UserId = t1.Id, AccommodationId = acc2.Id, RoomTypeId = room3.Id,
                        GuestFullName = t1.FullName, GuestEmail = t1.Email!, GuestPhone = "0901234567",
                        CheckInDate = DateTime.Now.AddDays(21), CheckOutDate = DateTime.Now.AddDays(23),
                        NumberOfGuests = 2, NumberOfRooms = 1,
                        TotalAmount = 2400000, CommissionRate = 0.08m, CommissionAmount = 192000,
                        BookingStatus = BookingStatus.Pending
                    };
                    context.Bookings.AddRange(booking1, booking2, booking3);
                    await context.SaveChangesAsync();

                    // Seed GuestPreferences for confirmed bookings
                    var pref1 = new GuestPreference
                    {
                        BookingId = booking1.Id,
                        RawText = "Chúng tôi kỷ niệm 5 năm ngày cưới. Vợ tôi bị dị ứng hải sản. Muốn phòng yên tĩnh và cần đưa đón sân bay.",
                        HasFoodAllergy = true, FoodAllergyDetail = "Dị ứng hải sản",
                        DietPreference = "Không hải sản", RoomPreference = "Phòng yên tĩnh, tầng cao",
                        SpecialOccasion = "Kỷ niệm 5 năm ngày cưới",
                        NeedAirportPickup = true, NeedDecoration = true,
                        ConsentToShareWithHotel = true, AiProcessedAt = DateTime.Now
                    };
                    var pref2 = new GuestPreference
                    {
                        BookingId = booking2.Id,
                        RawText = "Tôi ăn chay trường. Muốn phòng không có mùi nồng, tránh tinh dầu mạnh.",
                        DietPreference = "Ăn chay trường (vegan)",
                        RoomPreference = "Không mùi nồng, không tinh dầu mạnh",
                        TravelPurpose = "Nghỉ ngơi cuối tuần",
                        ConsentToShareWithHotel = true, AiProcessedAt = DateTime.Now
                    };
                    context.GuestPreferences.AddRange(pref1, pref2);
                    await context.SaveChangesAsync();

                    // Seed GuestInsights
                    var insight1 = new GuestInsight
                    {
                        GuestPreferenceId = pref1.Id,
                        Summary = "Khách kỷ niệm 5 năm ngày cưới. Vợ dị ứng hải sản (ưu tiên cao). Cần phòng yên tĩnh, đưa đón sân bay và trang trí phòng.",
                        PriorityLevel = PriorityLevel.High
                    };
                    var insight2 = new GuestInsight
                    {
                        GuestPreferenceId = pref2.Id,
                        Summary = "Khách ăn chay trường, nhạy cảm với mùi. Cần chuẩn bị menu chay và phòng không mùi nồng.",
                        PriorityLevel = PriorityLevel.Medium
                    };
                    context.GuestInsights.AddRange(insight1, insight2);
                    await context.SaveChangesAsync();

                    // Seed Tags for insight1
                    context.GuestInsightTags.AddRange(
                        new GuestInsightTag { GuestInsightId = insight1.Id, Category = TagCategory.Allergy, TagName = "Dị ứng hải sản", Severity = SeverityLevel.High, Description = "Vợ khách dị ứng hải sản" },
                        new GuestInsightTag { GuestInsightId = insight1.Id, Category = TagCategory.SpecialOccasion, TagName = "Kỷ niệm ngày cưới", Severity = SeverityLevel.Medium },
                        new GuestInsightTag { GuestInsightId = insight1.Id, Category = TagCategory.RoomPreference, TagName = "Phòng yên tĩnh", Severity = SeverityLevel.Medium },
                        new GuestInsightTag { GuestInsightId = insight2.Id, Category = TagCategory.FoodDiet, TagName = "Ăn chay trường", Severity = SeverityLevel.High },
                        new GuestInsightTag { GuestInsightId = insight2.Id, Category = TagCategory.RoomPreference, TagName = "Không mùi nồng", Severity = SeverityLevel.Medium }
                    );

                    // Seed Tasks
                    context.GuestTasks.AddRange(
                        new GuestTask { GuestInsightId = insight1.Id, Department = Department.Kitchen, Title = "Chuẩn bị menu không hải sản", Description = "Vợ khách dị ứng hải sản. Loại bỏ toàn bộ hải sản khỏi bữa ăn." },
                        new GuestTask { GuestInsightId = insight1.Id, Department = Department.CustomerService, Title = "Trang trí phòng kỷ niệm", Description = "Khách kỷ niệm 5 năm ngày cưới. Chuẩn bị hoa, nến và rượu." },
                        new GuestTask { GuestInsightId = insight1.Id, Department = Department.Reception, Title = "Sắp xếp phòng yên tĩnh tầng cao", Description = "Khách yêu cầu phòng yên tĩnh, ưu tiên tầng cao." },
                        new GuestTask { GuestInsightId = insight2.Id, Department = Department.Kitchen, Title = "Chuẩn bị menu chay", Description = "Khách ăn chay trường. Chuẩn bị menu hoàn toàn từ thực vật." },
                        new GuestTask { GuestInsightId = insight2.Id, Department = Department.Housekeeping, Title = "Phòng không mùi nồng", Description = "Không dùng tinh dầu mạnh, nước xịt phòng. Khách nhạy cảm với mùi." }
                    );

                    // Seed UpsellSuggestions
                    context.UpsellSuggestions.AddRange(
                        new UpsellSuggestion { GuestInsightId = insight1.Id, Title = "Trang trí phòng kỷ niệm", Reason = "Khách kỷ niệm 5 năm ngày cưới", EstimatedPrice = 800000, Status = UpsellStatus.Suggested },
                        new UpsellSuggestion { GuestInsightId = insight1.Id, Title = "Đưa đón sân bay", Reason = "Khách yêu cầu đưa đón sân bay", EstimatedPrice = 1200000, Status = UpsellStatus.Suggested }
                    );

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}
