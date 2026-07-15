using System.ComponentModel.DataAnnotations;

namespace RoomBook.Data;

public sealed class BookingEditorModel : IValidatableObject
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Title { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    public TimeOnly StartTime { get; set; } = new(9, 0);
    public TimeOnly EndTime { get; set; } = new(10, 0);

    [MinLength(1, ErrorMessage = "Select at least one room.")]
    public List<int> RoomIds { get; set; } = [];

    public List<string> ParticipantIds { get; set; } = [];

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (EndTime <= StartTime)
            yield return new ValidationResult("End time must be after start time.", [nameof(EndTime)]);
        if (StartTime.Minute % 15 != 0 || EndTime.Minute % 15 != 0)
            yield return new ValidationResult("Use 15-minute increments.");
        if (EndTime.ToTimeSpan() - StartTime.ToTimeSpan() > TimeSpan.FromHours(12))
            yield return new ValidationResult("A booking cannot exceed 12 hours.");
    }

    public DateTime StartUtc => DateTime.SpecifyKind(Date.ToDateTime(StartTime), DateTimeKind.Utc);
    public DateTime EndUtc => DateTime.SpecifyKind(Date.ToDateTime(EndTime), DateTimeKind.Utc);
}
