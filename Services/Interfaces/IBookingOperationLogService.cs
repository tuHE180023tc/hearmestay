using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HearMeStay.Models;
using HearMeStay.Models.Enums;

namespace HearMeStay.Services.Interfaces
{
    public interface IBookingOperationLogService
    {
        Task AddLogAsync(int bookingId, string actorUserId, string actorRole, BookingOperationActionType actionType, BookingStatus? oldStatus, BookingStatus? newStatus, string? note, string? internalNote, string? nextAction, DateTime? nextActionDueAt);
        Task<List<BookingOperationLog>> GetLogsByBookingIdAsync(int bookingId);
        Task AddInternalNoteAsync(int bookingId, string actorUserId, string actorRole, string internalNote);
        Task UpdateNextActionAsync(int bookingId, string actorUserId, string actorRole, string nextAction, DateTime? nextActionDueAt);
        Task LogStatusChangeAsync(int bookingId, string actorUserId, string actorRole, BookingStatus? oldStatus, BookingStatus? newStatus, BookingOperationActionType actionType, string? note, string? nextAction = null);
        Task LogSystemActionAsync(int bookingId, BookingOperationActionType actionType, string? note);
    }
}
