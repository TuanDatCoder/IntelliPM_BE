namespace IntelliPM.Data.DTOs.Account.Response
{
    public class AccountResponseDTO
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public string? Position { get; set; }
        public DateOnly? DateOfBirth { get; set; }
        public string? Status { get; set; }
        public string? Role { get; set; }
        public string? Picture { get; set; }
    }

}
