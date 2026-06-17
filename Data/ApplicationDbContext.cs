using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using HearMeStay.Models;

namespace HearMeStay.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Accommodation> Accommodations { get; set; }
        public DbSet<AccommodationImage> AccommodationImages { get; set; }
        public DbSet<RoomType> RoomTypes { get; set; }
        public DbSet<RoomImage> RoomImages { get; set; }
        public DbSet<Amenity> Amenities { get; set; }
        public DbSet<AccommodationAmenity> AccommodationAmenities { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<BookingOperationLog> BookingOperationLogs { get; set; }
        public DbSet<GuestPreference> GuestPreferences { get; set; }
        public DbSet<GuestInsight> GuestInsights { get; set; }
        public DbSet<GuestInsightTag> GuestInsightTags { get; set; }
        public DbSet<GuestTask> GuestTasks { get; set; }
        public DbSet<AddOnService> AddOnServices { get; set; }
        public DbSet<UpsellSuggestion> UpsellSuggestions { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<CommissionTransaction> CommissionTransactions { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<SubscriptionPlan> SubscriptionPlans { get; set; }
        public DbSet<PartnerSubscription> PartnerSubscriptions { get; set; }
        public DbSet<SubscriptionPayment> SubscriptionPayments { get; set; }
        public DbSet<UserPreferenceProfile> UserPreferenceProfiles { get; set; }
        public DbSet<UserPreferenceTag> UserPreferenceTags { get; set; }
        public DbSet<WebsiteVisitLog> WebsiteVisitLogs { get; set; }
        public DbSet<AccommodationViewLog> AccommodationViewLogs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // AccommodationAmenity composite key
            builder.Entity<AccommodationAmenity>()
                .HasKey(aa => new { aa.AccommodationId, aa.AmenityId });

            builder.Entity<AccommodationAmenity>()
                .HasOne(aa => aa.Accommodation)
                .WithMany(a => a.AccommodationAmenities)
                .HasForeignKey(aa => aa.AccommodationId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<AccommodationAmenity>()
                .HasOne(aa => aa.Amenity)
                .WithMany(a => a.AccommodationAmenities)
                .HasForeignKey(aa => aa.AmenityId)
                .OnDelete(DeleteBehavior.Cascade);

            // Accommodation -> Owner
            builder.Entity<Accommodation>()
                .HasOne(a => a.Owner)
                .WithMany(u => u.Accommodations)
                .HasForeignKey(a => a.OwnerId)
                .OnDelete(DeleteBehavior.Restrict);

            // Accommodation -> Images
            builder.Entity<AccommodationImage>()
                .HasOne(ai => ai.Accommodation)
                .WithMany(a => a.Images)
                .HasForeignKey(ai => ai.AccommodationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Accommodation -> RoomTypes
            builder.Entity<RoomType>()
                .HasOne(rt => rt.Accommodation)
                .WithMany(a => a.RoomTypes)
                .HasForeignKey(rt => rt.AccommodationId)
                .OnDelete(DeleteBehavior.Cascade);

            // RoomType -> RoomImages
            builder.Entity<RoomImage>()
                .HasOne(ri => ri.RoomType)
                .WithMany(rt => rt.Images)
                .HasForeignKey(ri => ri.RoomTypeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Accommodation -> AddOnServices
            builder.Entity<AddOnService>()
                .HasOne(s => s.Accommodation)
                .WithMany(a => a.AddOnServices)
                .HasForeignKey(s => s.AccommodationId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking -> User
            builder.Entity<Booking>()
                .HasOne(b => b.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(b => b.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking -> Accommodation
            builder.Entity<Booking>()
                .HasOne(b => b.Accommodation)
                .WithMany(a => a.Bookings)
                .HasForeignKey(b => b.AccommodationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking -> RoomType
            builder.Entity<Booking>()
                .HasOne(b => b.RoomType)
                .WithMany(rt => rt.Bookings)
                .HasForeignKey(b => b.RoomTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Booking -> OperationLogs
            builder.Entity<BookingOperationLog>()
                .HasOne(l => l.Booking)
                .WithMany(b => b.OperationLogs)
                .HasForeignKey(l => l.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // Booking -> GuestPreference (1-to-1)
            builder.Entity<GuestPreference>()
                .HasOne(gp => gp.Booking)
                .WithOne(b => b.GuestPreference)
                .HasForeignKey<GuestPreference>(gp => gp.BookingId)
                .OnDelete(DeleteBehavior.Cascade);

            // GuestPreference -> GuestInsight (1-to-1)
            builder.Entity<GuestInsight>()
                .HasOne(gi => gi.GuestPreference)
                .WithOne(gp => gp.GuestInsight)
                .HasForeignKey<GuestInsight>(gi => gi.GuestPreferenceId)
                .OnDelete(DeleteBehavior.Cascade);

            // GuestInsight -> Tags
            builder.Entity<GuestInsightTag>()
                .HasOne(t => t.GuestInsight)
                .WithMany(gi => gi.Tags)
                .HasForeignKey(t => t.GuestInsightId)
                .OnDelete(DeleteBehavior.Cascade);

            // GuestInsight -> Tasks
            builder.Entity<GuestTask>()
                .HasOne(t => t.GuestInsight)
                .WithMany(gi => gi.Tasks)
                .HasForeignKey(t => t.GuestInsightId)
                .OnDelete(DeleteBehavior.Cascade);

            // GuestInsight -> UpsellSuggestions
            builder.Entity<UpsellSuggestion>()
                .HasOne(u => u.GuestInsight)
                .WithMany(gi => gi.UpsellSuggestions)
                .HasForeignKey(u => u.GuestInsightId)
                .OnDelete(DeleteBehavior.Cascade);

            // UpsellSuggestion -> AddOnService (optional)
            builder.Entity<UpsellSuggestion>()
                .HasOne(u => u.AddOnService)
                .WithMany()
                .HasForeignKey(u => u.AddOnServiceId)
                .OnDelete(DeleteBehavior.SetNull);

            // Review -> Booking (1-to-1)
            builder.Entity<Review>()
                .HasOne(r => r.Booking)
                .WithOne(b => b.Review)
                .HasForeignKey<Review>(r => r.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> User
            builder.Entity<Review>()
                .HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Review -> Accommodation
            builder.Entity<Review>()
                .HasOne(r => r.Accommodation)
                .WithMany(a => a.Reviews)
                .HasForeignKey(r => r.AccommodationId)
                .OnDelete(DeleteBehavior.Restrict);

            // CommissionTransaction -> Booking (1-to-1)
            builder.Entity<CommissionTransaction>()
                .HasOne(c => c.Booking)
                .WithOne(b => b.CommissionTransaction)
                .HasForeignKey<CommissionTransaction>(c => c.BookingId)
                .OnDelete(DeleteBehavior.Restrict);

            // CommissionTransaction -> Accommodation
            builder.Entity<CommissionTransaction>()
                .HasOne(c => c.Accommodation)
                .WithMany()
                .HasForeignKey(c => c.AccommodationId)
                .OnDelete(DeleteBehavior.Restrict);

            // Notification -> User
            builder.Entity<Notification>()
                .HasOne(n => n.User)
                .WithMany(u => u.Notifications)
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            builder.Entity<Accommodation>()
                .HasIndex(a => a.Slug).IsUnique();

            builder.Entity<Booking>()
                .HasIndex(b => b.BookingCode).IsUnique();

            builder.Entity<Accommodation>()
                .HasIndex(a => a.City);

            builder.Entity<Accommodation>()
                .HasIndex(a => a.Province);

            builder.Entity<Accommodation>()
                .HasIndex(a => a.Status);
        }
    }
}
