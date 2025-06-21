public class ProjectMemberResponseDTO
{
    public int Id { get; set; }
    public int AccountId { get; set; }
    public int ProjectId { get; set; }
    public DateTime JoinedAt { get; set; }
    public DateTime InvitedAt { get; set; }
    public string Status { get; set; }
    public List<ProjectPositionResponseDTO>? ProjectPositions { get; set; }
}