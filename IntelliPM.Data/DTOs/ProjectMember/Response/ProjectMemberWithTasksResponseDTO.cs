using IntelliPM.Data.DTOs.Task.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMember.Response
{
    public class ProjectMemberWithTasksResponseDTO
    {
        public int Id { get; set; }
        public int AccountId { get; set; }
        public string FullName { get; set; } = null!;
        public string Username { get; set; } = null!;
        public string AccountPicture {  get; set; } = null!;
        public decimal? HourlyRate { get; set; }
        public decimal? WorkingHoursPerDay { get; set; }
        public List<TaskBasicDTO> Tasks { get; set; } = new();
    }
}
