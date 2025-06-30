using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskAssignment.Response
{
    public class TaskAssignmentResponseDTO
    {
        public int Id { get; set; }
        public string TaskId { get; set; }
        public int AccountId { get; set; }
        public string? Status { get; set; }
        public DateTime? AssignedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public decimal? HourlyRate { get; set; }
        public string? AccountFullname { get; set; } 
        public string? AccountPicture { get; set; }
    }
}
