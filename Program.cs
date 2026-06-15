using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using HearMeStay.Data;
using HearMeStay.Models;
using HearMeStay.Services;
using HearMeStay.Services.Interfaces;

namespace HearMeStay
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add DbContext
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Add Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 6;
                options.User.RequireUniqueEmail = true;
                options.SignIn.RequireConfirmedEmail = false;
            })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

            // Configure cookie
            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login";
                options.LogoutPath = "/Account/Logout";
                options.AccessDeniedPath = "/Account/AccessDenied";
                options.ExpireTimeSpan = TimeSpan.FromDays(7);
            });

            // Add Google Authentication (placeholder - only active when ClientId is configured)
            var googleClientId = builder.Configuration["Authentication:Google:ClientId"];
            if (!string.IsNullOrEmpty(googleClientId))
            {
                builder.Services.AddAuthentication()
                    .AddGoogle(options =>
                    {
                        options.ClientId = googleClientId;
                        options.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"] ?? "";
                    });
            }

            // Register application services
            builder.Services.AddScoped<IBookingService, BookingService>();
            builder.Services.AddScoped<INotificationService, NotificationService>();
            builder.Services.AddScoped<IFileUploadService, FileUploadService>();
            builder.Services.AddScoped<IReportService, ReportService>();
            builder.Services.AddScoped<ISubscriptionService, SubscriptionService>();
            builder.Services.AddHostedService<SubscriptionBackgroundService>();

            // Conditional AI provider registration
            var aiProvider = builder.Configuration["AI:Provider"] ?? "Mock";
            if (aiProvider == "OpenAI")
                builder.Services.AddScoped<IPreferenceAnalysisService, OpenAIPreferenceAnalysisService>();
            else
                builder.Services.AddScoped<IPreferenceAnalysisService, MockPreferenceAnalysisService>();

            builder.Services.AddControllersWithViews();

            var app = builder.Build();

            // Seed data
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    await context.Database.MigrateAsync();
                    await SeedData.InitializeAsync(services);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "Lỗi khi seed data.");
                }
            }

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            // Area routing
            app.MapControllerRoute(
                name: "areas",
                pattern: "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            await app.RunAsync();
        }
    }
}
