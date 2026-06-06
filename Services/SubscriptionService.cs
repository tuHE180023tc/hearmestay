using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HearMeStay.Data;
using HearMeStay.Models;
using HearMeStay.Models.Enums;
using HearMeStay.Services.Interfaces;

namespace HearMeStay.Services
{
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;

        public SubscriptionService(ApplicationDbContext context, INotificationService notificationService)
        {
            _context = context;
            _notificationService = notificationService;
        }

        public async Task<List<SubscriptionPlan>> GetAllPlansAsync()
        {
            return await _context.SubscriptionPlans.Where(p => p.IsActive).ToListAsync();
        }

        public async Task<SubscriptionPlan?> GetPlanByIdAsync(int id)
        {
            return await _context.SubscriptionPlans.FindAsync(id);
        }

        public async Task<PartnerSubscription?> GetActiveSubscriptionAsync(string partnerId)
        {
            return await _context.PartnerSubscriptions
                .Include(s => s.SubscriptionPlan)
                .Where(s => s.PartnerId == partnerId && s.Status == SubscriptionStatus.Active && s.EndDate >= DateTime.Now)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<PartnerSubscription> CreateSubscriptionAsync(string partnerId, int planId)
        {
            var plan = await _context.SubscriptionPlans.FindAsync(planId);
            if (plan == null) throw new Exception("Plan not found");

            var subscription = new PartnerSubscription
            {
                PartnerId = partnerId,
                SubscriptionPlanId = planId,
                Status = plan.PricePerMonth == 0 ? SubscriptionStatus.Active : SubscriptionStatus.PendingPayment,
                StartDate = DateTime.Now,
                EndDate = plan.PricePerMonth == 0 ? DateTime.Now.AddYears(10) : DateTime.Now.AddMonths(1) // Free is effectively forever, others 1 month
            };

            _context.PartnerSubscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            return subscription;
        }

        public async Task<SubscriptionPayment> CreatePaymentAsync(int subscriptionId, string method)
        {
            var sub = await _context.PartnerSubscriptions.Include(s => s.SubscriptionPlan).FirstOrDefaultAsync(s => s.Id == subscriptionId);
            if (sub == null) throw new Exception("Subscription not found");

            var payment = new SubscriptionPayment
            {
                PartnerSubscriptionId = subscriptionId,
                Amount = sub.SubscriptionPlan.PricePerMonth,
                PaymentMethod = method,
                PaymentStatus = SubscriptionPaymentStatus.Unpaid
            };

            _context.SubscriptionPayments.Add(payment);
            await _context.SaveChangesAsync();

            return payment;
        }

        public async Task<bool> SubmitPaymentProofAsync(int paymentId, string qrImageUrl, string transferContent, string? proofImageUrl)
        {
            var payment = await _context.SubscriptionPayments.Include(p => p.PartnerSubscription).FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) return false;

            payment.PaymentQrImageUrl = qrImageUrl;
            payment.PaymentTransferContent = transferContent;
            payment.PaymentProofImageUrl = proofImageUrl;
            payment.PaymentStatus = SubscriptionPaymentStatus.WaitingVerification;
            
            payment.PartnerSubscription.Status = SubscriptionStatus.PaymentVerificationPending;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> VerifyPaymentAsync(int paymentId, string adminId)
        {
            var payment = await _context.SubscriptionPayments.Include(p => p.PartnerSubscription).FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) return false;

            payment.PaymentStatus = SubscriptionPaymentStatus.Paid;
            payment.PaidAt = DateTime.Now;
            payment.VerifiedAt = DateTime.Now;
            payment.VerifiedBy = adminId;

            payment.PartnerSubscription.Status = SubscriptionStatus.Active;
            payment.PartnerSubscription.StartDate = DateTime.Now;
            payment.PartnerSubscription.EndDate = DateTime.Now.AddMonths(1);

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                payment.PartnerSubscription.PartnerId,
                "Gói dịch vụ đã được kích hoạt",
                "Thanh toán của bạn đã được xác nhận. Gói dịch vụ mới của bạn đã được kích hoạt.",
                "SubscriptionActivated");

            return true;
        }

        public async Task<bool> RejectPaymentAsync(int paymentId, string adminId)
        {
            var payment = await _context.SubscriptionPayments.Include(p => p.PartnerSubscription).FirstOrDefaultAsync(p => p.Id == paymentId);
            if (payment == null) return false;

            payment.PaymentStatus = SubscriptionPaymentStatus.Rejected;
            payment.VerifiedAt = DateTime.Now;
            payment.VerifiedBy = adminId;

            payment.PartnerSubscription.Status = SubscriptionStatus.Cancelled;

            await _context.SaveChangesAsync();

            await _notificationService.CreateNotificationAsync(
                payment.PartnerSubscription.PartnerId,
                "Thanh toán gói dịch vụ bị từ chối",
                "Chúng tôi không thể xác minh thanh toán của bạn. Vui lòng kiểm tra lại hoặc liên hệ hỗ trợ.",
                "SubscriptionPaymentRejected");

            return true;
        }

        public async Task HandleExpiredSubscriptionsAsync()
        {
            var expiredSubs = await _context.PartnerSubscriptions
                .Where(s => s.Status == SubscriptionStatus.Active && s.EndDate < DateTime.Now)
                .ToListAsync();

            foreach (var sub in expiredSubs)
            {
                sub.Status = SubscriptionStatus.Expired;
                await _notificationService.CreateNotificationAsync(
                    sub.PartnerId,
                    "Gói dịch vụ đã hết hạn",
                    "Gói dịch vụ của bạn đã hết hạn. Vui lòng gia hạn để tiếp tục sử dụng các tính năng cao cấp.",
                    "SubscriptionExpired");
            }

            await _context.SaveChangesAsync();
        }
    }
}
