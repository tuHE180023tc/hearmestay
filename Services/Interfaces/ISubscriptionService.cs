using System.Collections.Generic;
using System.Threading.Tasks;
using HearMeStay.Models;

namespace HearMeStay.Services.Interfaces
{
    public interface ISubscriptionService
    {
        Task<List<SubscriptionPlan>> GetAllPlansAsync();
        Task<SubscriptionPlan?> GetPlanByIdAsync(int id);
        Task<PartnerSubscription?> GetActiveSubscriptionAsync(string partnerId);
        Task<PartnerSubscription> CreateSubscriptionAsync(string partnerId, int planId);
        Task<SubscriptionPayment> CreatePaymentAsync(int subscriptionId, string method);
        Task<bool> SubmitPaymentProofAsync(int paymentId, string qrImageUrl, string transferContent, string? proofImageUrl);
        Task<bool> VerifyPaymentAsync(int paymentId, string adminId);
        Task<bool> RejectPaymentAsync(int paymentId, string adminId);
        Task HandleExpiredSubscriptionsAsync();
    }
}
