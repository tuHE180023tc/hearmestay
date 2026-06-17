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
    public class BookingOperationLogService : IBookingOperationLogService
    {
        private readonly ApplicationDbContext _context;

        public BookingOperationLogService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddLogAsync(int bookingId, string actorUserId, string actorRole, BookingOperationActionType actionType, BookingStatus? oldStatus, BookingStatus? newStatus, string? note, string? internalNote, string? nextAction, DateTime? nextActionDueAt)
        {
            var log = new BookingOperationLog
            {
                BookingId = bookingId,
                ActorUserId = actorUserId,
                ActorRole = actorRole,
                ActionType = actionType,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Note = note,
                InternalNote = internalNote,
                NextAction = nextAction,
                NextActionDueAt = nextActionDueAt,
                CreatedAt = DateTime.Now
            };

            _context.BookingOperationLogs.Add(log);

            // Cập nhật thông tin xử lý mới nhất vào Booking
            var booking = await _context.Bookings.FindAsync(bookingId);
            if (booking != null)
            {
                if (!string.IsNullOrEmpty(nextAction))
                    booking.NextAction = nextAction;
                
                if (nextActionDueAt.HasValue)
                    booking.NextActionDueAt = nextActionDueAt;
                
                if (!string.IsNullOrEmpty(internalNote))
                    booking.InternalNote = internalNote;

                if (actorRole != "System")
                {
                    booking.LastHandledByUserId = actorUserId;
                    booking.LastHandledAt = DateTime.Now;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<List<BookingOperationLog>> GetLogsByBookingIdAsync(int bookingId)
        {
            return await _context.BookingOperationLogs
                .Where(l => l.BookingId == bookingId)
                .OrderByDescending(l => l.CreatedAt)
                .ToListAsync();
        }

        public async Task AddInternalNoteAsync(int bookingId, string actorUserId, string actorRole, string internalNote)
        {
            await AddLogAsync(bookingId, actorUserId, actorRole, BookingOperationActionType.InternalNoteAdded, null, null, null, internalNote, null, null);
        }

        public async Task UpdateNextActionAsync(int bookingId, string actorUserId, string actorRole, string nextAction, DateTime? nextActionDueAt)
        {
            await AddLogAsync(bookingId, actorUserId, actorRole, BookingOperationActionType.NextActionUpdated, null, null, null, null, nextAction, nextActionDueAt);
        }

        public async Task LogStatusChangeAsync(int bookingId, string actorUserId, string actorRole, BookingStatus? oldStatus, BookingStatus? newStatus, BookingOperationActionType actionType, string? note, string? nextAction = null)
        {
            await AddLogAsync(bookingId, actorUserId, actorRole, actionType, oldStatus, newStatus, note, null, nextAction, null);
        }

        public async Task LogSystemActionAsync(int bookingId, BookingOperationActionType actionType, string? note)
        {
            await AddLogAsync(bookingId, "System", "System", actionType, null, null, note, null, null, null);
        }
    }
}
