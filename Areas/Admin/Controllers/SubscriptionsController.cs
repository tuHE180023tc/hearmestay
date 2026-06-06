using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HearMeStay.Data;
using HearMeStay.Models;
using HearMeStay.Models.Enums;
using HearMeStay.Services.Interfaces;
using System.Threading.Tasks;

namespace HearMeStay.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class SubscriptionsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public SubscriptionsController(ApplicationDbContext context, ISubscriptionService subscriptionService, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _subscriptionService = subscriptionService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var payments = await _context.SubscriptionPayments
                .Include(p => p.PartnerSubscription)
                .ThenInclude(ps => ps.Partner)
                .Include(p => p.PartnerSubscription.SubscriptionPlan)
                .Where(p => p.PaymentStatus == SubscriptionPaymentStatus.WaitingVerification)
                .OrderBy(p => p.CreatedAt)
                .ToListAsync();

            return View(payments);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Verify(int id)
        {
            var adminId = _userManager.GetUserId(User);
            var result = await _subscriptionService.VerifyPaymentAsync(id, adminId);
            if (result)
            {
                TempData["Success"] = "Đã xác nhận thanh toán và kích hoạt gói.";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi xác nhận.";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id)
        {
            var adminId = _userManager.GetUserId(User);
            var result = await _subscriptionService.RejectPaymentAsync(id, adminId);
            if (result)
            {
                TempData["Success"] = "Đã từ chối thanh toán.";
            }
            else
            {
                TempData["Error"] = "Có lỗi xảy ra khi từ chối.";
            }
            return RedirectToAction(nameof(Index));
        }
    }
}
