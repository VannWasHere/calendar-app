namespace RoomBook.Data.Models;

public class BookingParticipant
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser User { get; set; } = null!;
}
