using System.ComponentModel.DataAnnotations;

namespace RoomBook.Data.Models;

public class Room
{
    public int Id { get; set; }

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(120)]
    public string? Location { get; set; }

    [Required, StringLength(60)]
    public string Type { get; set; } = "Meeting Room";

    [Range(1, 10000)]
    public int Capacity { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public ICollection<Booking> Bookings { get; set; } = [];
    public ICollection<BookingRoom> AdditionalBookings { get; set; } = [];
}
