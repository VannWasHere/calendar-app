using Microsoft.EntityFrameworkCore;
using RoomBook.Data.Models;

namespace RoomBook.Data;

public static class BookingRules
{
    public static async Task<string?> GetConflictAsync(
        ApplicationDbContext db, int bookingId, IReadOnlyCollection<int> roomIds,
        DateTime startUtc, DateTime endUtc)
    {
        if (endUtc <= startUtc) return "End time must be after start time.";
        var exists = await db.Bookings.AnyAsync(x =>
            x.Id != bookingId && x.StartUtc < endUtc && x.EndUtc > startUtc &&
            (roomIds.Contains(x.RoomId) || x.AdditionalRooms.Any(r => roomIds.Contains(r.RoomId))));
        return exists ? "One or more rooms are unavailable during this time." : null;
    }
}
