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
                    FullName = "Nguyễn Văn HA�ng", EmailConfirmed = true,
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
                    FullName = "LA� Minh Tuấn", EmailConfirmed = true,
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
                    new SubscriptionPlan { Name = "Free Listing", PricePerMonth = 0, CommissionRate = 12.0, Features = "Đăng tối đa 1 nơi lưu trA,PhAn tAch AI cơ bản,Nhận booking cơ bản" },
                    new SubscriptionPlan { Name = "Professional", PricePerMonth = 1499000, CommissionRate = 8.0, Features = "KhAng giới hạn nơi lưu trA,PhAn tAch AI chuyAn sAu,Xuất bAo cAo doanh thu chi tiết,Hỗ trợ ưu tiAn 24/7" },
                    new SubscriptionPlan { Name = "Premium", PricePerMonth = 4999000, CommissionRate = 3.0, Features = "Tất cả của gAi Pro,Hỗ trợ marketing đa kAnh,TAy chỉnh luồng booking,TAch hợp API hệ thống PMS riAng" }
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

                var acc4 = new Accommodation
                {
                    OwnerId = p2.Id, Name = "Army Hotel",
                    Slug = "army-hotel",
                    Description = "Khách sạn Army Hotel mang đến không gian nghỉ dưỡng sang trọng, hiện đại và đẳng cấp ngay tại trung tâm thủ đô Hà Nội.",
                    Address = "1A Nguyễn Tri Phương, Điện Biên, Ba Đình", City = "Hà Nội", Province = "Hà Nội",
                    AccommodationType = AccommodationType.Hotel, StarRating = 4,
                    Phone = "02438233366", Email = "info@armyhotel.com",
                    CheckInTime = new TimeSpan(14, 0, 0), CheckOutTime = new TimeSpan(12, 0, 0),
                    CancellationPolicy = "Hủy miễn phí trước 24 giờ",
                    Status = AccommodationStatus.Approved, IsActive = true
                };

                var acc5 = new Accommodation
                {
                    OwnerId = p2.Id, Name = "Hòn Tằm Resort",
                    Slug = "hon-tam-resort",
                    Description = "Khu nghỉ dưỡng 5 sao sang trọng nằm trên đảo Hòn Tằm xinh đẹp, mang đến trải nghiệm nghỉ dưỡng tuyệt vời với không gian xanh mát và bãi biển riêng tư.",
                    Address = "Đảo Hòn Tằm, Vĩnh Nguyên", City = "Nha Trang", Province = "Khánh Hòa",
                    AccommodationType = AccommodationType.Resort, StarRating = 5,
                    Phone = "02583597777", Email = "v.res-HTNT@vinpearl.com",
                    CheckInTime = new TimeSpan(14, 0, 0), CheckOutTime = new TimeSpan(12, 0, 0),
                    CancellationPolicy = "Hủy miễn phí trước 48 giờ",
                    Status = AccommodationStatus.Approved, IsActive = true
                };

                var acc6 = new Accommodation
                {
                    OwnerId = p1.Id, Name = "Mandala Retreats Kim Bôi",
                    Slug = "mandala-retreats-kim-boi",
                    Description = "Khu nghỉ dưỡng chăm sóc sức khỏe cao cấp với nguồn khoáng nóng tự nhiên Kim Bôi, thiết kế độc đáo giao thoa giữa thiên nhiên và kiến trúc hiện đại.",
                    Address = "Thôn Mớ Đá, Kim Bôi", City = "Hòa Bình", Province = "Hòa Bình",
                    AccommodationType = AccommodationType.Resort, StarRating = 5,
                    Phone = "0888780696", Email = "support@amazingo.vn",
                    CheckInTime = new TimeSpan(15, 0, 0), CheckOutTime = new TimeSpan(12, 0, 0),
                    CancellationPolicy = "Hủy miễn phí trước 48 giờ",
                    Status = AccommodationStatus.Approved, IsActive = true
                };

                var acc7 = new Accommodation
                {
                    OwnerId = p2.Id, Name = "Prague Hotel HCM",
                    Slug = "prague-hotel-hcm",
                    Description = "Khách sạn hiện đại và tiện nghi tọa lạc ngay trung tâm thành phố Hồ Chí Minh, thuận tiện cho việc di chuyển, tham quan và mua sắm.",
                    Address = "Phạm Ngũ Lão, Bến Thành", City = "Hồ Chí Minh", Province = "Hồ Chí Minh",
                    AccommodationType = AccommodationType.Hotel, StarRating = 3,
                    Phone = "02839259925", Email = "info@praguehotelhcm.com",
                    CheckInTime = new TimeSpan(14, 0, 0), CheckOutTime = new TimeSpan(12, 0, 0),
                    CancellationPolicy = "Hủy miễn phí trước 24 giờ",
                    Status = AccommodationStatus.Approved, IsActive = true
                };

                var acc8 = new Accommodation
                {
                    OwnerId = p1.Id, Name = "Queen Ann Hotel HCM",
                    Slug = "queen-ann-hotel-hcm",
                    Description = "Khách sạn Queen Ann mang đến không gian lưu trú sang trọng, ấm cúng cùng dịch vụ chuyên nghiệp, là điểm dừng chân lý tưởng tại Sài Gòn.",
                    Address = "88 Bùi Thị Xuân, Bến Thành", City = "Hồ Chí Minh", Province = "Hồ Chí Minh",
                    AccommodationType = AccommodationType.Hotel, StarRating = 4,
                    Phone = "02839259001", Email = "info@queenannhotel.com",
                    CheckInTime = new TimeSpan(14, 0, 0), CheckOutTime = new TimeSpan(12, 0, 0),
                    CancellationPolicy = "Hủy miễn phí trước 24 giờ",
                    Status = AccommodationStatus.Approved, IsActive = true
                };

                context.Accommodations.AddRange(acc1, acc2, acc3, acc4, acc5, acc6, acc7, acc8);
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
                foreach (var am in allAmenities)
                    aaList.Add(new AccommodationAmenity { AccommodationId = acc4.Id, AmenityId = am.Id });
                foreach (var am in allAmenities.Take(8))
                    aaList.Add(new AccommodationAmenity { AccommodationId = acc5.Id, AmenityId = am.Id });
                foreach (var am in allAmenities.Take(10))
                    aaList.Add(new AccommodationAmenity { AccommodationId = acc6.Id, AmenityId = am.Id });
                foreach (var am in allAmenities.Take(5))
                    aaList.Add(new AccommodationAmenity { AccommodationId = acc7.Id, AmenityId = am.Id });
                foreach (var am in allAmenities.Take(7))
                    aaList.Add(new AccommodationAmenity { AccommodationId = acc8.Id, AmenityId = am.Id });
                context.AccommodationAmenities.AddRange(aaList);

                // Seed AccommodationImages (placeholders)
                context.AccommodationImages.AddRange(
                    new AccommodationImage { AccommodationId = acc1.Id, ImageUrl = "/images/placeholder-villa.svg", IsMain = true },
                    new AccommodationImage { AccommodationId = acc2.Id, ImageUrl = "/images/placeholder-homestay.svg", IsMain = true },
                    new AccommodationImage { AccommodationId = acc3.Id, ImageUrl = "/images/placeholder-hotel.svg", IsMain = true },
                    // Army Hotel — ảnh từ thư mục Cơ sở vật chất
                    new AccommodationImage { AccommodationId = acc4.Id, ImageUrl = "/List danh sách khách sạn/ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG/Cơ sở vật chất/53117932.jpg", IsMain = true },
                    new AccommodationImage { AccommodationId = acc4.Id, ImageUrl = "/List danh sách khách sạn/ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG/Cơ sở vật chất/60021175.jpg", IsMain = false },
                    new AccommodationImage { AccommodationId = acc4.Id, ImageUrl = "/List danh sách khách sạn/ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG/Cơ sở vật chất/60021176.jpg", IsMain = false },
                    new AccommodationImage { AccommodationId = acc4.Id, ImageUrl = "/List danh sách khách sạn/ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG/Cơ sở vật chất/60021180.jpg", IsMain = false },
                    new AccommodationImage { AccommodationId = acc4.Id, ImageUrl = "/List danh sách khách sạn/ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG/Cơ sở vật chất/60021200.jpg", IsMain = false },
                    new AccommodationImage { AccommodationId = acc4.Id, ImageUrl = "/List danh sách khách sạn/ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG/Cơ sở vật chất/60021206.jpg", IsMain = false },
                    new AccommodationImage { AccommodationId = acc4.Id, ImageUrl = "/List danh sách khách sạn/ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG/Cơ sở vật chất/60021344.jpg", IsMain = false },
                    new AccommodationImage { AccommodationId = acc4.Id, ImageUrl = "/List danh sách khách sạn/ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG/Cơ sở vật chất/60021396.jpg", IsMain = false }
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
                var room6 = new RoomType
                {
                    AccommodationId = acc4.Id, Name = "Deluxe 3 người (Deluxe Triple)",
                    Description = "Nội thất hiện đại, không gian ấm cúng, phòng Deluxe với hai giường ngủ cỡ lớn là sự lựa chọn hoàn hảo dành cho bạn trong những chuyến đi xa cùng gia đình hoặc bạn bè. Đặc biệt, các trang thiết bị cao cấp trong phòng có thể đáp ứng mọi nhu cầu của bạn, khiến kỳ nghỉ của bạn trở nên tuyệt vời hơn bao giờ hết. Giá phòng trên chưa bao gồm 10% VAT và 5% phí dịch vụ. Dịch vụ phòng: Phục vụ phòng 24/7, Giặt là, Giặt lấy ngay, Gọi điện đánh thức, Gọi điện thoại trong nước và quốc tế.",
                    PricePerNight = 2510000, Capacity = 3, TotalRooms = 10, AvailableRooms = 8,
                    BedType = "2 giường đôi lớn", RoomSize = 45, HasPrivateBathroom = true,
                    IsQuietRoom = true
                };
                var room7 = new RoomType
                {
                    AccommodationId = acc4.Id, Name = "Phòng Presidential Suite",
                    Description = "Phòng Tổng thống, có diện tích 198m2, là sự lựa chọn hoàn hảo cho những ai đang tìm kiếm một trải nghiệm xa hoa, độc đáo. Căn phòng là sự kết hợp hài hòa giữa màu sắc tươi sáng, nội thất cao cấp và không gian rộng rãi, tràn đầy sức sống. Đặc biệt, phòng có khu bếp riêng và một ban công tuyệt đẹp, nhìn xuống tuyến phố Phan Đình Phùng, một trong những tuyến phố đẹp nhất của thủ đô Hà Nội. Căn phòng này sẽ mang đến cho bạn cảm giác thư thái trong một không gian vô cùng lãng mạn. Giá phòng trên chưa bao gồm 10% VAT và 5% phí dịch vụ. Dịch vụ phòng: Phục vụ phòng 24/7, Giặt khô là hơi, Giặt lấy ngay, Gọi điện đánh thức, Gọi điện trong nước và quốc tế.",
                    PricePerNight = 27750000, Capacity = 4, TotalRooms = 2, AvailableRooms = 1,
                    BedType = "King", RoomSize = 198, HasPrivateBathroom = true,
                    IsQuietRoom = true, SupportsAllergyRequest = true
                };
                var room8 = new RoomType
                {
                    AccommodationId = acc4.Id, Name = "Phòng Suite",
                    Description = "Một không gian thoáng đãng, tràn ngập ánh sáng tự nhiên, phòng Suite bao gồm một phòng khách được thiết kế trang nhã và một phòng ngủ rộng rãi, ấm áp. Căn phòng hứa hẹn đem đến cho bạn sự thoải mái, tiện nghi với các trang thiết bị hiện cao cấp, hiện đại. Nghỉ ngơi, thư giãn tại căn phòng này sẽ là một trải nghiệm khó quên dành cho bạn. Giá phòng trên chưa bao gồm 10% VAT và 5% phí dịch vụ. Dịch vụ phòng: Phục vụ phòng 24/7, Giặt khô là hơi, Giặt lấy ngay, Gọi điện đánh thức, Gọi điện trong nước và quốc tế.",
                    PricePerNight = 4030000, Capacity = 2, TotalRooms = 5, AvailableRooms = 3,
                    BedType = "King", RoomSize = 65, HasPrivateBathroom = true,
                    IsQuietRoom = true
                };
                var room9 = new RoomType
                {
                    AccommodationId = acc4.Id, Name = "Phòng Superior 2 người",
                    Description = "Ấm cúng, giản dị và tiện nghi, phòng Superior với thiết kế trang nhã và không gian yên tĩnh, là tất cả những gì bạn mong đợi khi đến với một khách sạn 5 sao. Bạn có thể lựa chọn phòng ngủ 01 giường đôi hoặc 02 giường đơn tùy theo nhu cầu sử dụng. Giá phòng trên chưa bao gồm 10% VAT và 5% phí dịch vụ. Dịch vụ phòng: Phục vụ phòng 24/7, Giặt là, Giặt lấy ngay, Gọi điện đánh thức, Gọi điện thoại trong nước và quốc tế.",
                    PricePerNight = 1900000, Capacity = 2, TotalRooms = 15, AvailableRooms = 10,
                    BedType = "1 King hoặc 2 đơn", RoomSize = 35, HasPrivateBathroom = true
                };

                context.RoomTypes.AddRange(room1, room2, room3, room4, room5, room6, room7, room8, room9);

                var room10 = new RoomType { AccommodationId = acc5.Id, Name = "Deluxe Bungalow hướng vườn giường đôi", Description = "Bungalow tiện nghi với thiết kế gần gũi thiên nhiên. Dịch vụ phòng: Điều hòa nhiệt độ, Nước khoáng miễn phí, Máy sấy tóc, Tivi, Wifi miễn phí.", PricePerNight = 1500000, Capacity = 2, TotalRooms = 10, AvailableRooms = 10, BedType = "1 giường đôi", RoomSize = 40, HasPrivateBathroom = true };
                var room11 = new RoomType { AccommodationId = acc5.Id, Name = "Executive Suite Hướng biển (Executive Suite Ocean View)", Description = "Phòng Suite sang trọng hướng thẳng ra biển Đông xanh ngắt. Dịch vụ phòng: Điều hòa nhiệt độ, Nước khoáng miễn phí, Máy sấy tóc, Tivi, Wifi miễn phí.", PricePerNight = 3200000, Capacity = 2, TotalRooms = 5, AvailableRooms = 5, BedType = "1 giường đôi", RoomSize = 65, HasPrivateBathroom = true };
                var room12 = new RoomType { AccommodationId = acc5.Id, Name = "Phòng Forestal Deluxe 2 giường đơn", Description = "Nằm ẩn mình trong rừng cây xanh mát, mang lại sự yên bình. Dịch vụ phòng: Điều hòa nhiệt độ, Nước khoáng miễn phí, Máy sấy tóc, Tivi, Wifi miễn phí.", PricePerNight = 1350000, Capacity = 2, TotalRooms = 15, AvailableRooms = 15, BedType = "2 giường đơn", RoomSize = 35, HasPrivateBathroom = true };

                var room13 = new RoomType { AccommodationId = acc6.Id, Name = "Deluxe 2 giường hướng sân vườn (Deluxe Garden Twin)", Description = "Phòng có tầm nhìn ra vườn cây tĩnh lặng, phù hợp cho sự thư giãn. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 1660000, Capacity = 2, TotalRooms = 10, AvailableRooms = 10, BedType = "2 giường đơn", RoomSize = 35, HasPrivateBathroom = true };
                var room14 = new RoomType { AccommodationId = acc6.Id, Name = "Phòng Loại Sang Hướng Vườn Giường Đôi (Deluxe Garden Double)", Description = "Phòng Deluxe giường đôi thoải mái giữa khu vườn xanh mát. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 1660000, Capacity = 2, TotalRooms = 10, AvailableRooms = 10, BedType = "1 giường đôi", RoomSize = 35, HasPrivateBathroom = true };
                var room15 = new RoomType { AccommodationId = acc6.Id, Name = "Phòng Loại Sang Hướng Bể Bơi 2 Giường Đơn (Deluxe Twin Pool View)", Description = "Phòng view bể bơi tươi mát với hai giường đơn. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 1800000, Capacity = 2, TotalRooms = 8, AvailableRooms = 8, BedType = "2 giường đơn", RoomSize = 38, HasPrivateBathroom = true };
                var room16 = new RoomType { AccommodationId = acc6.Id, Name = "Phòng Hai Giường Đơn Thương Gia Hướng Núi (Executive Twin room- Mountain view)", Description = "Phòng thương gia, view núi non hùng vĩ. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 2580000, Capacity = 2, TotalRooms = 5, AvailableRooms = 5, BedType = "2 giường đơn", RoomSize = 45, HasPrivateBathroom = true };
                var room17 = new RoomType { AccommodationId = acc6.Id, Name = "Phòng Đôi Thương Gia Hướng Núi (Executive Mountain View Double Room)", Description = "Phòng thương gia cao cấp, hướng núi. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 2580000, Capacity = 2, TotalRooms = 5, AvailableRooms = 5, BedType = "1 giường đôi", RoomSize = 45, HasPrivateBathroom = true };
                var room18 = new RoomType { AccommodationId = acc6.Id, Name = "Phòng Executive Hướng Bể Bơi Hai Giường Đơn (Executive Pool View Twin Room)", Description = "Hạng phòng Executive view bể bơi, tiện nghi đẳng cấp. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 2720000, Capacity = 2, TotalRooms = 5, AvailableRooms = 5, BedType = "2 giường đơn", RoomSize = 45, HasPrivateBathroom = true };
                var room19 = new RoomType { AccommodationId = acc6.Id, Name = "Phòng Executive Nhìn Ra Hồ Bơi (Executive Pool View Room)", Description = "Phòng Executive view hồ bơi với giường đôi cực lớn. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 2720000, Capacity = 2, TotalRooms = 5, AvailableRooms = 5, BedType = "1 giường đôi", RoomSize = 45, HasPrivateBathroom = true };
                var room20 = new RoomType { AccommodationId = acc6.Id, Name = "Phòng Suite Hướng núi (Suite Mountain View)", Description = "Phòng Suite siêu rộng 90m2, thiết kế ấn tượng. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 3470000, Capacity = 2, TotalRooms = 2, AvailableRooms = 2, BedType = "1 giường đôi", RoomSize = 90, HasPrivateBathroom = true };
                var room21 = new RoomType { AccommodationId = acc6.Id, Name = "Phòng Suite Hướng hồ bơi (Suite Pool View)", Description = "Phòng Suite cao cấp view hồ bơi, nội thất hoàng gia. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 3680000, Capacity = 2, TotalRooms = 2, AvailableRooms = 2, BedType = "1 giường đôi", RoomSize = 90, HasPrivateBathroom = true };
                var room22 = new RoomType { AccommodationId = acc6.Id, Name = "La Yên Villa ( Villa 2 BR Private Pool)", Description = "Villa 2 phòng ngủ với bể bơi riêng tư, lý tưởng cho gia đình. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 6630000, Capacity = 4, TotalRooms = 2, AvailableRooms = 2, BedType = "2 giường đôi", RoomSize = 150, HasPrivateBathroom = true };
                var room23 = new RoomType { AccommodationId = acc6.Id, Name = "La Minh Villa ( 2-Bedroom Villa with Private Pool)", Description = "Villa La Minh đẳng cấp, rộng rãi với tiện ích 5 sao. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 7510000, Capacity = 4, TotalRooms = 2, AvailableRooms = 2, BedType = "2 giường đôi", RoomSize = 170, HasPrivateBathroom = true };
                var room24 = new RoomType { AccommodationId = acc6.Id, Name = "La Sen Villa  (Villa 3 BR Private Pool)", Description = "Villa La Sen lớn nhất với 3 phòng ngủ và bể bơi vô cực. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 10600000, Capacity = 6, TotalRooms = 1, AvailableRooms = 1, BedType = "3 giường đôi", RoomSize = 277, HasPrivateBathroom = true };
                var room25 = new RoomType { AccommodationId = acc6.Id, Name = "Family Suite Mountain View", Description = "Phòng Suite dành cho gia đình lớn, view núi cực đẹp. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 5680000, Capacity = 4, TotalRooms = 2, AvailableRooms = 2, BedType = "2 phòng ngủ", RoomSize = 135, HasPrivateBathroom = true };
                var room26 = new RoomType { AccommodationId = acc6.Id, Name = "Family Suite Pool View", Description = "Phòng Suite dành cho gia đình, view bể bơi khoáng mát lạnh. Dịch vụ phòng: Nước suối set up trên phòng, Nước welcome khi khách đến nhận phòng, Bữa sáng buffet cao cấp tại Nhà hàng, Miễn phí sử dụng internet tốc độ cao, Miễn phí sử dụng bể bơi khoáng nóng.", PricePerNight = 6010000, Capacity = 4, TotalRooms = 2, AvailableRooms = 2, BedType = "2 phòng ngủ", RoomSize = 135, HasPrivateBathroom = true };

                var room27 = new RoomType { AccommodationId = acc7.Id, Name = "Superior giường đôi Có cửa sổ", Description = "Phòng Superior êm ái, ánh sáng tự nhiên ngập tràn. Dịch vụ phòng: Wifi miễn phí, Điều hòa nhiệt độ, Nước khoáng miễn phí, Máy sấy tóc, Tivi.", PricePerNight = 750000, Capacity = 2, TotalRooms = 10, AvailableRooms = 10, BedType = "1 giường đôi", RoomSize = 25, HasPrivateBathroom = true };
                var room28 = new RoomType { AccommodationId = acc7.Id, Name = "Phòng Superior Giường Đôi Không có Cửa sổ", Description = "Phòng yên tĩnh, thiết kế tối giản, tiết kiệm chi phí. Dịch vụ phòng: Wifi miễn phí, Điều hòa nhiệt độ, Nước khoáng miễn phí, Máy sấy tóc, Tivi.", PricePerNight = 700000, Capacity = 2, TotalRooms = 5, AvailableRooms = 5, BedType = "1 giường đôi", RoomSize = 22, HasPrivateBathroom = true };
                var room29 = new RoomType { AccommodationId = acc7.Id, Name = "Phòng Superior 2 Giường đơn Không có Cửa sổ", Description = "Lựa chọn tiện lợi cho nhóm bạn bè với 2 giường đơn êm ái. Dịch vụ phòng: Wifi miễn phí, Điều hòa nhiệt độ, Nước khoáng miễn phí, Máy sấy tóc, Tivi.", PricePerNight = 750000, Capacity = 2, TotalRooms = 5, AvailableRooms = 5, BedType = "2 giường đơn", RoomSize = 22, HasPrivateBathroom = true };
                var room30 = new RoomType { AccommodationId = acc7.Id, Name = "Deluxe giường đôi", Description = "Phòng Deluxe rộng rãi, nội thất cao cấp và tầm nhìn thoáng đãng. Dịch vụ phòng: Wifi miễn phí, Điều hòa nhiệt độ, Nước khoáng miễn phí, Máy sấy tóc, Tivi.", PricePerNight = 850000, Capacity = 2, TotalRooms = 10, AvailableRooms = 10, BedType = "1 giường đôi", RoomSize = 30, HasPrivateBathroom = true };
                var room31 = new RoomType { AccommodationId = acc7.Id, Name = "Deluxe 2 giường đơn", Description = "Phòng Deluxe cao cấp với 2 giường đơn, cửa sổ lớn. Dịch vụ phòng: Wifi miễn phí, Điều hòa nhiệt độ, Nước khoáng miễn phí, Máy sấy tóc, Tivi.", PricePerNight = 850000, Capacity = 2, TotalRooms = 10, AvailableRooms = 10, BedType = "2 giường đơn", RoomSize = 30, HasPrivateBathroom = true };
                var room32 = new RoomType { AccommodationId = acc7.Id, Name = "Phòng Gia đình (1 giường đôi và 1 giường đơn)", Description = "Phòng lớn thích hợp cho gia đình 3 người, ấm cúng và đầy đủ tiện nghi. Dịch vụ phòng: Wifi miễn phí, Điều hòa nhiệt độ, Nước khoáng miễn phí, Máy sấy tóc, Tivi.", PricePerNight = 1050000, Capacity = 3, TotalRooms = 5, AvailableRooms = 5, BedType = "1 đôi + 1 đơn", RoomSize = 35, HasPrivateBathroom = true };

                var room33 = new RoomType { AccommodationId = acc8.Id, Name = "Superior No Window ( 1 giường đôi hoặc 2 giường đơn )", Description = "Phòng Superior ấm cúng, thiết kế trang nhã. Dịch vụ phòng: Phục vụ phòng 24/7, Giặt là, Wifi miễn phí, Điều hòa nhiệt độ, Két an toàn.", PricePerNight = 900000, Capacity = 2, TotalRooms = 15, AvailableRooms = 15, BedType = "1 đôi hoặc 2 đơn", RoomSize = 25, HasPrivateBathroom = true };
                var room34 = new RoomType { AccommodationId = acc8.Id, Name = "Phòng Superior 3 Người (Superior Triple Room)", Description = "Không gian rộng rãi, tiện nghi, phù hợp cho nhóm 3 khách. Dịch vụ phòng: Phục vụ phòng 24/7, Giặt là, Wifi miễn phí, Điều hòa nhiệt độ, Két an toàn.", PricePerNight = 1250000, Capacity = 3, TotalRooms = 10, AvailableRooms = 10, BedType = "1 đôi + 1 đơn", RoomSize = 32, HasPrivateBathroom = true };
                var room35 = new RoomType { AccommodationId = acc8.Id, Name = "Phòng Giường Đôi Lớn Loại Sang (Deluxe Room Queen)", Description = "Phòng Deluxe giường đôi cao cấp với nội thất hiện đại và tầm nhìn đẹp. Dịch vụ phòng: Phục vụ phòng 24/7, Giặt là, Wifi miễn phí, Điều hòa nhiệt độ, Két an toàn.", PricePerNight = 1000000, Capacity = 2, TotalRooms = 15, AvailableRooms = 15, BedType = "1 giường đôi", RoomSize = 30, HasPrivateBathroom = true };
                var room36 = new RoomType { AccommodationId = acc8.Id, Name = "Phòng Hai Giường Đơn Loại Sang (Deluxe Twin Room)", Description = "Phòng Deluxe Twin mang đến sự thoải mái tối đa với 2 giường đơn êm ái. Dịch vụ phòng: Phục vụ phòng 24/7, Giặt là, Wifi miễn phí, Điều hòa nhiệt độ, Két an toàn.", PricePerNight = 1000000, Capacity = 2, TotalRooms = 10, AvailableRooms = 10, BedType = "2 giường đơn", RoomSize = 30, HasPrivateBathroom = true };
                var room37 = new RoomType { AccommodationId = acc8.Id, Name = "Phòng Deluxe 3 người (Deluxe Triple Rooms)", Description = "Phòng gia đình hạng sang dành cho 3 người, trải nghiệm nghỉ dưỡng khác biệt. Dịch vụ phòng: Phục vụ phòng 24/7, Giặt là, Wifi miễn phí, Điều hòa nhiệt độ, Két an toàn.", PricePerNight = 1350000, Capacity = 3, TotalRooms = 5, AvailableRooms = 5, BedType = "1 đôi + 1 đơn", RoomSize = 35, HasPrivateBathroom = true };
                var room38 = new RoomType { AccommodationId = acc8.Id, Name = "Phòng Premier 3 người (Premier Triple Room)", Description = "Hạng phòng Premier đẳng cấp với diện tích lớn, thiết kế hoàng gia. Dịch vụ phòng: Phục vụ phòng 24/7, Giặt là, Wifi miễn phí, Điều hòa nhiệt độ, Két an toàn.", PricePerNight = 1400000, Capacity = 3, TotalRooms = 5, AvailableRooms = 5, BedType = "1 đôi + 1 đơn", RoomSize = 40, HasPrivateBathroom = true };
                var room39 = new RoomType { AccommodationId = acc8.Id, Name = "Phòng Đẳng Cấp Cho Gia Đình (Premier Family)", Description = "Trải nghiệm tuyệt đỉnh cho gia đình tại Queen Ann với nội thất xa xỉ. Dịch vụ phòng: Phục vụ phòng 24/7, Giặt là, Wifi miễn phí, Điều hòa nhiệt độ, Két an toàn.", PricePerNight = 1650000, Capacity = 4, TotalRooms = 5, AvailableRooms = 5, BedType = "2 giường đôi", RoomSize = 45, HasPrivateBathroom = true };

                context.RoomTypes.AddRange(room10, room11, room12, room13, room14, room15, room16, room17, room18, room19, room20, room21, room22, room23, room24, room25, room26, room27, room28, room29, room30, room31, room32, room33, room34, room35, room36, room37, room38, room39);

                // Seed AddOnServices
                context.AddOnServices.AddRange(
                    new AddOnService { AccommodationId = acc1.Id, Name = "Trang trí phòng kỷ niệm", Description = "Trang trí hoa, nến và rượu vang cho dịp đặc biệt.", Price = 800000, ServiceType = ServiceType.Decoration },
                    new AddOnService { AccommodationId = acc1.Id, Name = "Đưa đón sân bay", Description = "Xe riêng đưa đón sân bay Vân Đồn.", Price = 1200000, ServiceType = ServiceType.AirportPickup },
                    new AddOnService { AccommodationId = acc1.Id, Name = "Tiệc BBQ riêng", Description = "Tiệc BBQ hải sản tươi sống tại villa.", Price = 1500000, ServiceType = ServiceType.BBQ },
                    new AddOnService { AccommodationId = acc3.Id, Name = "Spa truyền thống", Description = "Liệu trình spa thảo mộc truyền thống.", Price = 600000, ServiceType = ServiceType.Spa },
                    new AddOnService { AccommodationId = acc3.Id, Name = "Tour phố cổ", Description = "Tour đi bộ khám phá phố cổ Hà Nội.", Price = 350000, ServiceType = ServiceType.Tour }
                );

                // Seed RoomImages
                await context.SaveChangesAsync();
                var roomImgs = new List<RoomImage>
                {
                    new RoomImage { RoomTypeId = room1.Id, ImageUrl = "/images/placeholder-room.svg", IsMain = true },
                    new RoomImage { RoomTypeId = room2.Id, ImageUrl = "/images/placeholder-room.svg", IsMain = true },
                    new RoomImage { RoomTypeId = room3.Id, ImageUrl = "/images/placeholder-room.svg", IsMain = true },
                    new RoomImage { RoomTypeId = room4.Id, ImageUrl = "/images/placeholder-room.svg", IsMain = true },
                    new RoomImage { RoomTypeId = room5.Id, ImageUrl = "/images/placeholder-room.svg", IsMain = true }
                };
                context.RoomImages.AddRange(roomImgs);

                // Dynamic image seeding
                string wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                string[] imgExts = { ".jpg", ".png", ".jpeg", ".webp" };

                // Find actual facility folder name (handles case differences)
                string FindFacilityFolder(string hotelFolder)
                {
                    if (!Directory.Exists(hotelFolder)) return "";
                    var match = Directory.GetDirectories(hotelFolder)
                        .FirstOrDefault(d => Path.GetFileName(d).Equals("Cơ sở vật chất", StringComparison.OrdinalIgnoreCase)
                                          || Path.GetFileName(d).Equals("cơ sở vật chất", StringComparison.OrdinalIgnoreCase));
                    return match ?? "";
                }
                
                void SeedHotelImages(Accommodation acc, string hotelDirName)
                {
                    string hotelFolder = Path.Combine(wwwrootPath, "List danh sách khách sạn", hotelDirName);
                    string facilityFolder = FindFacilityFolder(hotelFolder);
                    if (!string.IsNullOrEmpty(facilityFolder) && Directory.Exists(facilityFolder))
                    {
                        string actualFolderName = Path.GetFileName(facilityFolder);
                        var files = Directory.GetFiles(facilityFolder, "*.*").Where(f => imgExts.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase))).ToList();
                        bool isMain = true;
                        foreach (var f in files)
                        {
                            string relPath = "/List danh sách khách sạn/" + hotelDirName + "/" + actualFolderName + "/" + Path.GetFileName(f);
                            context.AccommodationImages.Add(new AccommodationImage { AccommodationId = acc.Id, ImageUrl = relPath, IsMain = isMain });
                            isMain = false;
                        }
                    }
                    if (!context.AccommodationImages.Local.Any(ai => ai.AccommodationId == acc.Id))
                    {
                        context.AccommodationImages.Add(new AccommodationImage { AccommodationId = acc.Id, ImageUrl = "/images/placeholder-hotel.svg", IsMain = true });
                    }
                }

                void SeedRoomImagesDynamic(RoomType room, string hotelDirName, string roomFolderName)
                {
                    string folderPath = Path.Combine(wwwrootPath, "List danh sách khách sạn", hotelDirName, roomFolderName);
                    if (Directory.Exists(folderPath))
                    {
                        string actualRoomFolder = Path.GetFileName(folderPath);
                        var files = Directory.GetFiles(folderPath, "*.*").Where(f => imgExts.Any(ext => f.EndsWith(ext, StringComparison.OrdinalIgnoreCase))).ToList();
                        bool isMain = true;
                        foreach (var f in files)
                        {
                            string fn = Path.GetFileName(f).ToLower();
                            if (fn.StartsWith("thông tin phòng") || fn.StartsWith("contact") || fn.StartsWith("dịch vụ")) continue;
                            string relPath = "/List danh sách khách sạn/" + hotelDirName + "/" + actualRoomFolder + "/" + Path.GetFileName(f);
                            context.RoomImages.Add(new RoomImage { RoomTypeId = room.Id, ImageUrl = relPath, IsMain = isMain });
                            isMain = false;
                        }
                    }
                    if (!context.RoomImages.Local.Any(ri => ri.RoomTypeId == room.Id))
                    {
                        context.RoomImages.Add(new RoomImage { RoomTypeId = room.Id, ImageUrl = "/images/placeholder-room.svg", IsMain = true });
                    }
                }

                SeedHotelImages(acc4, "ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG");
                SeedHotelImages(acc5, "Hòn Tằm Resort");
                SeedHotelImages(acc6, "Mandala Retreat Kim Bôi");
                SeedHotelImages(acc7, "Prague Hotel HCM");
                SeedHotelImages(acc8, "Queen Ann Hotel HCM");

                // Army Hotel rooms
                SeedRoomImagesDynamic(room6, "ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG", "Deluxe 3 người (Deluxe Triple)");
                SeedRoomImagesDynamic(room7, "ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG", "Phòng Presidential Suite");
                SeedRoomImagesDynamic(room8, "ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG", "Phòng Suite");
                SeedRoomImagesDynamic(room9, "ARMY HOTEL - 1A NGUYỄN TRI PHƯƠNG", "Phòng Superior 2 người  ( 2 giường đơn- 1 giường đôi )");

                // Hòn Tằm Resort rooms
                SeedRoomImagesDynamic(room10, "Hòn Tằm Resort", "Deluxe Bungalow hướng vườn giường đôi");
                SeedRoomImagesDynamic(room11, "Hòn Tằm Resort", "Executive Suite Hướng biển (Executive Suite Ocean View)");
                SeedRoomImagesDynamic(room12, "Hòn Tằm Resort", "Phòng Forestal Deluxe 2 giường đơn");

                // Mandala Retreat Kim Bôi rooms
                SeedRoomImagesDynamic(room13, "Mandala Retreat Kim Bôi", "Deluxe 2 giường hướng sân vườn (Deluxe Garden Twin)");
                SeedRoomImagesDynamic(room14, "Mandala Retreat Kim Bôi", "Phòng Loại Sang Hướng Vườn Giường Đôi (Deluxe Garden Double)");
                SeedRoomImagesDynamic(room15, "Mandala Retreat Kim Bôi", "Phòng Loại Sang Hướng Bể Bơi 2 Giường Đơn (Deluxe Twin Pool View)");
                SeedRoomImagesDynamic(room16, "Mandala Retreat Kim Bôi", "Phòng Hai Giường Đơn Thương Gia Hướng Núi (Executive Twin room- Mountain view)");
                SeedRoomImagesDynamic(room17, "Mandala Retreat Kim Bôi", "Phòng Đôi Thương Gia Hướng Núi (Executive Mountain View Double Room)");
                SeedRoomImagesDynamic(room18, "Mandala Retreat Kim Bôi", "Phòng Executive Hướng Bể Bơi Hai Giường Đơn (Executive Pool View Twin Room)");
                SeedRoomImagesDynamic(room19, "Mandala Retreat Kim Bôi", "Phòng Executive Nhìn Ra Hồ Bơi (Executive Pool View Room)");
                SeedRoomImagesDynamic(room20, "Mandala Retreat Kim Bôi", "Phòng Suite Hướng núi (Suite Mountain View)");
                SeedRoomImagesDynamic(room21, "Mandala Retreat Kim Bôi", "Phòng Suite Hướng hồ bơi (Suite Pool View)");
                SeedRoomImagesDynamic(room22, "Mandala Retreat Kim Bôi", "La Yên Villa ( Villa 2 BR Private Pool)");
                SeedRoomImagesDynamic(room23, "Mandala Retreat Kim Bôi", "La Minh Villa ( 2-Bedroom Villa with Private Pool)");
                SeedRoomImagesDynamic(room24, "Mandala Retreat Kim Bôi", "La Sen Villa  (Villa 3 BR Private Pool)");
                SeedRoomImagesDynamic(room25, "Mandala Retreat Kim Bôi", "Family Suite Mountain View");
                SeedRoomImagesDynamic(room26, "Mandala Retreat Kim Bôi", "Family Suite Pool View");

                // Prague Hotel HCM rooms
                SeedRoomImagesDynamic(room27, "Prague Hotel HCM", "Superior giường đôi Có cửa sổ");
                SeedRoomImagesDynamic(room28, "Prague Hotel HCM", "Phòng Superior Giường Đôi Không có Cửa sổ");
                SeedRoomImagesDynamic(room29, "Prague Hotel HCM", "Phòng Superior 2 Giường đơn Không có Cửa sổ");
                SeedRoomImagesDynamic(room30, "Prague Hotel HCM", "Deluxe giường đôi");
                SeedRoomImagesDynamic(room31, "Prague Hotel HCM", "Deluxe 2 giường đơn");
                SeedRoomImagesDynamic(room32, "Prague Hotel HCM", "Phòng Gia đình (1 giường đôi và 1 giường đơn)");

                // Queen Ann Hotel HCM rooms
                SeedRoomImagesDynamic(room33, "Queen Ann Hotel HCM", "Superior No Window ( 1 giường đôi hoặc 2 giường đơn )");
                SeedRoomImagesDynamic(room34, "Queen Ann Hotel HCM", "Phòng Superior 3 Người (Superior Triple Room)");
                SeedRoomImagesDynamic(room35, "Queen Ann Hotel HCM", "Phòng Giường Đôi Lớn Loại Sang (Deluxe Room Queen)");
                SeedRoomImagesDynamic(room36, "Queen Ann Hotel HCM", "Phòng Hai Giường Đơn Loại Sang (Deluxe Twin Room)");
                SeedRoomImagesDynamic(room37, "Queen Ann Hotel HCM", "Phòng Deluxe 3 người (Deluxe Triple Rooms)");
                SeedRoomImagesDynamic(room38, "Queen Ann Hotel HCM", "Phòng Premier 3 người (Premier Triple Room)");
                SeedRoomImagesDynamic(room39, "Queen Ann Hotel HCM", "Phòng Đẳng Cấp Cho Gia Đình (Premier Family)");

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
                        RawText = "ChA�ng tA�i kỷ niệm 5 năm ngA�y cưới. Vợ tA�i bị dị ứng hải sản. Muốn phA�ng yA�n tĩnh vA� cần đưa đA�n sA�n bay.",
                        HasFoodAllergy = true, FoodAllergyDetail = "Dị ứng hải sản",
                        DietPreference = "KhA�ng hải sản", RoomPreference = "PhA�ng yA�n tĩnh, tầng cao",
                        SpecialOccasion = "Kỷ niệm 5 năm ngA�y cưới",
                        NeedAirportPickup = true, NeedDecoration = true,
                        ConsentToShareWithHotel = true, AiProcessedAt = DateTime.Now
                    };
                    var pref2 = new GuestPreference
                    {
                        BookingId = booking2.Id,
                        RawText = "TA�i ăn chay trường. Muốn phA�ng khA�ng cA� mA�i nồng, trA�nh tinh dầu mạnh.",
                        DietPreference = "Ăn chay trường (vegan)",
                        RoomPreference = "KhA�ng mA�i nồng, khA�ng tinh dầu mạnh",
                        TravelPurpose = "Nghỉ ngơi cuối tuần",
                        ConsentToShareWithHotel = true, AiProcessedAt = DateTime.Now
                    };
                    context.GuestPreferences.AddRange(pref1, pref2);
                    await context.SaveChangesAsync();

                    // Seed GuestInsights
                    var insight1 = new GuestInsight
                    {
                        GuestPreferenceId = pref1.Id,
                        Summary = "KhA�ch kỷ niệm 5 năm ngA�y cưới. Vợ dị ứng hải sản (ưu tiA�n cao). Cần phA�ng yA�n tĩnh, đưa đA�n sA�n bay vA� trang trA� phA�ng.",
                        PriorityLevel = PriorityLevel.High
                    };
                    var insight2 = new GuestInsight
                    {
                        GuestPreferenceId = pref2.Id,
                        Summary = "KhA�ch ăn chay trường, nhạy cảm với mA�i. Cần chuẩn bị menu chay vA� phA�ng khA�ng mA�i nồng.",
                        PriorityLevel = PriorityLevel.Medium
                    };
                    context.GuestInsights.AddRange(insight1, insight2);
                    await context.SaveChangesAsync();

                    // Seed Tags for insight1
                    context.GuestInsightTags.AddRange(
                        new GuestInsightTag { GuestInsightId = insight1.Id, Category = TagCategory.Allergy, TagName = "Dị ứng hải sản", Severity = SeverityLevel.High, Description = "Vợ khA�ch dị ứng hải sản" },
                        new GuestInsightTag { GuestInsightId = insight1.Id, Category = TagCategory.SpecialOccasion, TagName = "Kỷ niệm ngA�y cưới", Severity = SeverityLevel.Medium },
                        new GuestInsightTag { GuestInsightId = insight1.Id, Category = TagCategory.RoomPreference, TagName = "PhA�ng yA�n tĩnh", Severity = SeverityLevel.Medium },
                        new GuestInsightTag { GuestInsightId = insight2.Id, Category = TagCategory.FoodDiet, TagName = "Ăn chay trường", Severity = SeverityLevel.High },
                        new GuestInsightTag { GuestInsightId = insight2.Id, Category = TagCategory.RoomPreference, TagName = "KhA�ng mA�i nồng", Severity = SeverityLevel.Medium }
                    );

                    // Seed Tasks
                    context.GuestTasks.AddRange(
                        new GuestTask { GuestInsightId = insight1.Id, Department = Department.Kitchen, Title = "Chuẩn bị menu khA�ng hải sản", Description = "Vợ khA�ch dị ứng hải sản. Loại bỏ toA�n bộ hải sản khỏi bữa ăn." },
                        new GuestTask { GuestInsightId = insight1.Id, Department = Department.CustomerService, Title = "Trang trA� phA�ng kỷ niệm", Description = "KhA�ch kỷ niệm 5 năm ngA�y cưới. Chuẩn bị hoa, nến vA� rượu." },
                        new GuestTask { GuestInsightId = insight1.Id, Department = Department.Reception, Title = "Sắp xếp phA�ng yA�n tĩnh tầng cao", Description = "KhA�ch yA�u cầu phA�ng yA�n tĩnh, ưu tiA�n tầng cao." },
                        new GuestTask { GuestInsightId = insight2.Id, Department = Department.Kitchen, Title = "Chuẩn bị menu chay", Description = "KhA�ch ăn chay trường. Chuẩn bị menu hoA�n toA�n từ thực vật." },
                        new GuestTask { GuestInsightId = insight2.Id, Department = Department.Housekeeping, Title = "PhA�ng khA�ng mA�i nồng", Description = "KhA�ng dA�ng tinh dầu mạnh, nước xịt phA�ng. KhA�ch nhạy cảm với mA�i." }
                    );

                    // Seed UpsellSuggestions
                    context.UpsellSuggestions.AddRange(
                        new UpsellSuggestion { GuestInsightId = insight1.Id, Title = "Trang trA� phA�ng kỷ niệm", Reason = "KhA�ch kỷ niệm 5 năm ngA�y cưới", EstimatedPrice = 800000, Status = UpsellStatus.Suggested },
                        new UpsellSuggestion { GuestInsightId = insight1.Id, Title = "Đưa đA�n sA�n bay", Reason = "KhA�ch yA�u cầu đưa đA�n sA�n bay", EstimatedPrice = 1200000, Status = UpsellStatus.Suggested }
                    );

                    await context.SaveChangesAsync();
                }
            }
        }
    }
}

