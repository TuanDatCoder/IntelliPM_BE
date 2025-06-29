namespace IntelliPM.Data.DTOs.MeetingRescheduleRequest.Response;

public class MeetingRescheduleRequestResponseDTO
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public int RequesterId { get; set; }
    public DateTime RequestedDate { get; set; }
    public string? Reason { get; set; }
    public string Status { get; set; } = null!;
    public int? PmId { get; set; }
    public DateTime? PmProposedDate { get; set; }
    public string? PmNote { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}