using System;
using System.Collections.Generic;
using IntelliPM.Data.DTOs.ProjectMember.Response;
using IntelliPM.Data.DTOs.ProjectPosition.Response;

namespace IntelliPM.Data.DTOs.Account.Response
{
    public class ProfileResponseDTO
    {
        // Thông tin cơ bản
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

        // ====== Thông tin thống kê Project ======
        public int TotalProjects { get; set; }                 
        public int CompletedProjects { get; set; }              
        public int InProgressProjects { get; set; }             
        public int UpcomingProjects { get; set; }
        public int ActiveProjects { get; set; }
        public int CancelledProjects { get; set; }

        public List<ProjectByAccountResponseDTO> ProjectList { get; set; } = new(); 

        // ====== Thông tin Positions ======
        public int TotalPositions { get; set; }                 
        public List<string> PositionsList { get; set; } = new(); 
        public List<ProjectPositionResponseDTO> RecentPositions { get; set; } = new(); 
    }



}
