namespace IntelliPM.Data.DTOs.MeetingLog.Response;

public class MeetingLogResponseDTO
{
    public int Id { get; set; }
    public int MeetingId { get; set; }
    public int AccountId { get; set; }
    public string Action { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public string AccountName { get; set; } = null!;
}