namespace IntelliPM.Data.DTOs.MeetingLog.Request;

public class MeetingLogRequestDTO
{
    public int MeetingId { get; set; }
    public int AccountId { get; set; }
    public string Action { get; set; } = null!;
}