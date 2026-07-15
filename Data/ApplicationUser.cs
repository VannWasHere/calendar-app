using Microsoft.AspNetCore.Identity;

namespace RoomBook.Data;

public class ApplicationUser : IdentityUser
{
    public string DisplayName { get; set; } = string.Empty;
    public ICollection<Models.BookingParticipant> BookingParticipations { get; set; } = [];
}
