using Microsoft.EntityFrameworkCore;
using RoomBook.Data.Models;

namespace RoomBook.Data;

public sealed class BookingService(IDbContextFactory<ApplicationDbContext> factory)
{
    public async Task<BookingEditorModel?> GetEditorAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var booking = await db.Bookings.AsNoTracking()
            .Include(x => x.AdditionalRooms).Include(x => x.Participants)
            .SingleOrDefaultAsync(x => x.Id == id);
        if (booking is null) return null;
        return new BookingEditorModel
        {
            Id = booking.Id,
            Title = booking.Title,
            Description = booking.Notes,
            Date = DateOnly.FromDateTime(booking.StartUtc),
            StartTime = TimeOnly.FromDateTime(booking.StartUtc),
            EndTime = TimeOnly.FromDateTime(booking.EndUtc),
            RoomIds = [booking.RoomId, .. booking.AdditionalRooms.Select(x => x.RoomId)],
            ParticipantIds = booking.Participants.Select(x => x.UserId).ToList()
        };
    }

    public async Task<string?> CheckConflictAsync(int excludeId, IReadOnlyCollection<int> roomIds, DateTime startUtc, DateTime endUtc)
    {
        if (roomIds.Count == 0) return null;
        await using var db = await factory.CreateDbContextAsync();
        return await BookingRules.GetConflictAsync(db, excludeId, roomIds, startUtc, endUtc);
    }

    public async Task<BookingResult> SaveAsync(BookingEditorModel model, string creatorId)
    {
        await using var db = await factory.CreateDbContextAsync();
        var roomIds = model.RoomIds.Distinct().ToList();
        if (roomIds.Count == 0) return BookingResult.Fail("Select at least one room.");
        var validRoomCount = await db.Rooms.CountAsync(x => roomIds.Contains(x.Id) && x.IsActive);
        if (validRoomCount != roomIds.Count) return BookingResult.Fail("One or more rooms are unavailable.");
        var conflict = await BookingRules.GetConflictAsync(db, model.Id, roomIds, model.StartUtc, model.EndUtc);
        if (conflict is not null) return BookingResult.Fail(conflict);

        var participantIds = model.ParticipantIds.Distinct().ToList();
        var validParticipantIds = await db.Users.Where(x => participantIds.Contains(x.Id)).Select(x => x.Id).ToListAsync();
        if (validParticipantIds.Count != participantIds.Count) return BookingResult.Fail("One or more participants no longer exist.");

        Booking booking;
        if (model.Id == 0)
        {
            booking = new Booking { CreatedById = creatorId };
            db.Bookings.Add(booking);
        }
        else
        {
            booking = await db.Bookings.Include(x => x.AdditionalRooms).Include(x => x.Participants)
                .SingleOrDefaultAsync(x => x.Id == model.Id) ?? throw new InvalidOperationException("Booking not found.");
            db.BookingRooms.RemoveRange(booking.AdditionalRooms);
            db.BookingParticipants.RemoveRange(booking.Participants);
        }

        booking.Title = model.Title.Trim();
        booking.Notes = string.IsNullOrWhiteSpace(model.Description) ? null : model.Description.Trim();
        booking.StartUtc = model.StartUtc;
        booking.EndUtc = model.EndUtc;
        booking.RoomId = roomIds[0];
        booking.AdditionalRooms = roomIds.Skip(1).Select(id => new BookingRoom { RoomId = id }).ToList();
        booking.Participants = validParticipantIds.Select(id => new BookingParticipant { UserId = id }).ToList();

        try
        {
            await db.SaveChangesAsync();
            return BookingResult.Ok(booking.Id);
        }
        catch (DbUpdateException)
        {
            return BookingResult.Fail("The booking could not be saved. Refresh and try again.");
        }
    }

    public async Task<BookingResult> MoveAsync(int id, int roomId, DateTime startUtc, DateTime endUtc)
    {
        await using var db = await factory.CreateDbContextAsync();
        var booking = await db.Bookings.Include(x => x.AdditionalRooms).SingleOrDefaultAsync(x => x.Id == id);
        if (booking is null) return BookingResult.Fail("Booking not found.");
        var roomIds = booking.AdditionalRooms.Select(x => x.RoomId).Where(x => x != roomId).Prepend(roomId).ToList();
        var conflict = await BookingRules.GetConflictAsync(db, id, roomIds, startUtc, endUtc);
        if (conflict is not null) return BookingResult.Fail(conflict);
        booking.RoomId = roomId;
        booking.StartUtc = DateTime.SpecifyKind(startUtc, DateTimeKind.Utc);
        booking.EndUtc = DateTime.SpecifyKind(endUtc, DateTimeKind.Utc);
        await db.SaveChangesAsync();
        return BookingResult.Ok(id);
    }

    public async Task DeleteAsync(int id)
    {
        await using var db = await factory.CreateDbContextAsync();
        var booking = await db.Bookings.FindAsync(id);
        if (booking is null) return;
        db.Bookings.Remove(booking);
        await db.SaveChangesAsync();
    }
}

public sealed record BookingResult(bool Succeeded, string? Error, int BookingId)
{
    public static BookingResult Ok(int id) => new(true, null, id);
    public static BookingResult Fail(string error) => new(false, error, 0);
}
