using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RoomBook.Data.Models;

namespace RoomBook.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<Room> Rooms => Set<Room>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingRoom> BookingRooms => Set<BookingRoom>();
    public DbSet<BookingParticipant> BookingParticipants => Set<BookingParticipant>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>().ToTable("users");
        builder.Entity<IdentityRole>().ToTable("roles");
        builder.Entity<IdentityRoleClaim<string>>().ToTable("role_claims");
        builder.Entity<IdentityUserClaim<string>>().ToTable("user_claims");
        builder.Entity<IdentityUserLogin<string>>().ToTable("user_logins");
        builder.Entity<IdentityUserPasskey<string>>().ToTable("user_passkeys");
        builder.Entity<IdentityUserRole<string>>().ToTable("user_roles");
        builder.Entity<IdentityUserToken<string>>().ToTable("user_tokens");
        builder.Entity<Room>().ToTable("rooms");
        builder.Entity<Booking>().ToTable("bookings");
        builder.Entity<BookingRoom>().ToTable("booking_rooms");
        builder.Entity<BookingParticipant>().ToTable("booking_participants");

        builder.Entity<Room>().HasIndex(x => x.Name).IsUnique();
        builder.Entity<Booking>().HasIndex(x => new { x.RoomId, x.StartUtc });
        builder.Entity<Booking>().HasIndex(x => x.EndUtc);
        builder.Entity<Booking>().HasOne(x => x.Room).WithMany(x => x.Bookings).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);
        builder.Entity<Booking>().HasOne(x => x.CreatedBy).WithMany().HasForeignKey(x => x.CreatedById).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BookingRoom>().HasKey(x => new { x.BookingId, x.RoomId });
        builder.Entity<BookingRoom>().HasOne(x => x.Booking).WithMany(x => x.AdditionalRooms).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<BookingRoom>().HasOne(x => x.Room).WithMany(x => x.AdditionalBookings).HasForeignKey(x => x.RoomId).OnDelete(DeleteBehavior.Restrict);

        builder.Entity<BookingParticipant>().HasKey(x => new { x.BookingId, x.UserId });
        builder.Entity<BookingParticipant>().HasIndex(x => x.UserId);
        builder.Entity<BookingParticipant>().HasOne(x => x.Booking).WithMany(x => x.Participants).HasForeignKey(x => x.BookingId).OnDelete(DeleteBehavior.Cascade);
        builder.Entity<BookingParticipant>().HasOne(x => x.User).WithMany(x => x.BookingParticipations).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
    }
}
