namespace RoomBook.Data.Models;

public class BookingRoom
{
    public int BookingId { get; set; }
    public Booking Booking { get; set; } = null!;
    public int RoomId { get; set; }
    public Room Room { get; set; } = null!;
}
