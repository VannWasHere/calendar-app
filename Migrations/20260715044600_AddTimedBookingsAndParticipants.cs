using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoomBook.Migrations
{
    /// <inheritdoc />
    public partial class AddTimedBookingsAndParticipants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_bookings_RoomId_BookingDate",
                table: "bookings");

            migrationBuilder.AddColumn<DateTime>(
                name: "EndUtc",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartUtc",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE bookings
                SET "StartUtc" = ("BookingDate"::timestamp + time '09:00') AT TIME ZONE 'UTC',
                    "EndUtc" = ("BookingDate"::timestamp + time '10:00') AT TIME ZONE 'UTC';
                """);

            migrationBuilder.AlterColumn<DateTime>(
                name: "StartUtc",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "EndUtc",
                table: "bookings",
                type: "timestamp with time zone",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.DropColumn(
                name: "BookingDate",
                table: "bookings");

            migrationBuilder.CreateTable(
                name: "booking_participants",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_participants", x => new { x.BookingId, x.UserId });
                    table.ForeignKey(
                        name: "FK_booking_participants_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_booking_participants_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "booking_rooms",
                columns: table => new
                {
                    BookingId = table.Column<int>(type: "integer", nullable: false),
                    RoomId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_booking_rooms", x => new { x.BookingId, x.RoomId });
                    table.ForeignKey(
                        name: "FK_booking_rooms_bookings_BookingId",
                        column: x => x.BookingId,
                        principalTable: "bookings",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_booking_rooms_rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_bookings_EndUtc",
                table: "bookings",
                column: "EndUtc");

            migrationBuilder.CreateIndex(
                name: "IX_bookings_RoomId_StartUtc",
                table: "bookings",
                columns: new[] { "RoomId", "StartUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_booking_participants_UserId",
                table: "booking_participants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_booking_rooms_RoomId",
                table: "booking_rooms",
                column: "RoomId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "booking_participants");

            migrationBuilder.DropTable(
                name: "booking_rooms");

            migrationBuilder.DropIndex(
                name: "IX_bookings_EndUtc",
                table: "bookings");

            migrationBuilder.DropIndex(
                name: "IX_bookings_RoomId_StartUtc",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "EndUtc",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "StartUtc",
                table: "bookings");

            migrationBuilder.AddColumn<DateOnly>(
                name: "BookingDate",
                table: "bookings",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(1, 1, 1));

            migrationBuilder.CreateIndex(
                name: "IX_bookings_RoomId_BookingDate",
                table: "bookings",
                columns: new[] { "RoomId", "BookingDate" },
                unique: true);
        }
    }
}
