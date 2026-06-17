namespace HearMeStay.Models.Enums
{
    public enum BookingOperationActionType
    {
        BookingCreated,
        PreferenceSubmitted,
        AiAnalyzed,
        SentToHotelPartner,
        PartnerViewedBooking,
        PartnerConfirmedBooking,
        PartnerRejectedBooking,
        PaymentOpened,
        PaymentSubmitted,
        PaymentProofUploaded,
        PaymentVerified,
        PaymentRejected,
        BookingConfirmed,
        BookingCancelled,
        BookingCompleted,
        NoteAdded,
        InternalNoteAdded,
        NextActionUpdated,
        Reassigned
    }
}
