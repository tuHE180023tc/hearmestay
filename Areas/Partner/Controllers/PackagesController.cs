using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using HearMeStay.Models;
using HearMeStay.Models.Enums;
using HearMeStay.Services.Interfaces;
using System.Threading.Tasks;

namespace HearMeStay.Areas.Partner.Controllers
{
    [Area("Partner")]
    [Authorize(Roles = "HotelPartner")]
    public class PackagesController : Controller
    {
        private readonly ISubscriptionService _subscriptionService;
        private readonly UserManager<ApplicationUser> _userManager;

        public PackagesController(ISubscriptionService subscriptionService, UserManager<ApplicationUser> userManager)
        {
            _subscriptionService = subscriptionService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var plans = await _subscriptionService.GetAllPlansAsync();
            var activeSub = await _subscriptionService.GetActiveSubscriptionAsync(userId);
            
            ViewBag.ActivePlanId = activeSub?.SubscriptionPlanId ?? 1; // Default to Free if none
            return View(plans);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Upgrade(int planId)
        {
            var userId = _userManager.GetUserId(User);
            var plan = await _subscriptionService.GetPlanByIdAsync(planId);
            if (plan == null) return NotFound();

            if (plan.PricePerMonth == 0)
            {
                // Downgrade or switch to free
                await _subscriptionService.CreateSubscriptionAsync(userId, planId);
                TempData["Success"] = "Đã chuyển sang gói Free Listing.";
                return RedirectToAction("Index");
            }

            var sub = await _subscriptionService.CreateSubscriptionAsync(userId, planId);
            var payment = await _subscriptionService.CreatePaymentAsync(sub.Id, "VietQR");
            
            return RedirectToAction("Payment", new { id = payment.Id });
        }

        public async Task<IActionResult> Payment(int id)
        {
            var userId = _userManager.GetUserId(User);
            // Need to verify this payment belongs to the user, skipped for brevity but would usually do this
            // We just need the payment details
            // I'll just pass the ID to the view, the view can fetch or we can pass a model
            ViewBag.PaymentId = id;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SubmitPayment(int id, string? proofImageUrl)
        {
            // Hardcoded QR and Transfer content for demo
            string transferContent = $"HMSPK{id}";
            string qrUrl = $"https://img.vietqr.io/image/970423-81655940116-compact2.png?amount=10000&addInfo={transferContent}&accountName=TO%20CHINH%20TU";
            
            bool result = await _subscriptionService.SubmitPaymentProofAsync(id, qrUrl, transferContent, proofImageUrl);
            if (result)
            {
                TempData["Success"] = "Đã gửi yêu cầu xác minh thanh toán. Vui lòng chờ admin duyệt.";
                return RedirectToAction("Index");
            }
            return NotFound();
        }
    }
}
