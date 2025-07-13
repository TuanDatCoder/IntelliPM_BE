using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Project.Response
{
    public class WorkItemResponseDTO
    {
        public string Type { get; set; } // "Epic", "Task", "Subtask"
        public string Key { get; set; } // ID
        public string? TaskId { get; set; } // Chỉ áp dụng cho Subtask, null cho Epic và Task
        public string Summary { get; set; } // Name cho Epic, Title cho Task/Subtask
        public string? Status { get; set; }
        public int CommentCount { get; set; }
        public int? SprintId { get; set; }
        public List<AssigneeDTO> Assignees { get; set; } = new List<AssigneeDTO>();
        public DateTime? DueDate { get; set; } // PlannedEndDate cho Epic/Task, EndDate cho Subtask
        public List<string> Labels { get; set; } = new List<string>(); // Danh sách tên label
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string ReporterFullname { get; set; }
        public string? ReporterPicture { get; set; }
    }

    public class AssigneeDTO
    {
        public string Fullname { get; set; } = "Unknown"; 
        public string? Picture { get; set; } 
    }
}
