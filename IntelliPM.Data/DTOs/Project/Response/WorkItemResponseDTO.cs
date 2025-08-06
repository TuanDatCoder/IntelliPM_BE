using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Project.Response
{
    public class WorkItemResponseDTO
    {
        public int ProjectId { get; set; }
        public string Type { get; set; } 
        public string Key { get; set; } 
        public string? TaskId { get; set; } 
        public string Summary { get; set; } 
        public string? Status { get; set; }
        public int CommentCount { get; set; }
        public int? SprintId { get; set; }
        public string? SprintName { get; set; }
        public List<AssigneeDTO> Assignees { get; set; } = new List<AssigneeDTO>();
        public DateTime? DueDate { get; set; } 
        public List<string> Labels { get; set; } = new List<string>(); 
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int? ReporterId { get; set; }
        public string? ReporterFullname { get; set; }
        public string? ReporterPicture { get; set; }
    }

    public class AssigneeDTO
    {
        public int AccountId { get; set; }
        public string Fullname { get; set; } = "Unknown"; 
        public string? Picture { get; set; } 
    }





    public class EpicDTO
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public int CommentCount { get; set; }
        public int? SprintId { get; set; }
        public string SprintName { get; set; }
        public int? AssignedBy { get; set; }
        public string AssignedByFullname { get; set; }
        public string AssignedByPicture { get; set; }
        public DateTime EndDate { get; set; }
        public List<LabelDTO> Labels { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ReporterId { get; set; }
        public string ReporterFullname { get; set; }
        public string ReporterPicture { get; set; }
    }

    public class TaskDTO
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public int CommentCount { get; set; }
        public int? SprintId { get; set; }
        public string SprintName { get; set; }
        public List<TaskAssignmentDTO> TaskAssignments { get; set; }
        public DateTime PlannedEndDate { get; set; }
        public List<LabelDTO> Labels { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ReporterId { get; set; }
        public string ReporterFullname { get; set; }
        public string ReporterPicture { get; set; }
    }

    public class SubtaskDTO
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string TaskId { get; set; }
        public string Title { get; set; }
        public string Status { get; set; }
        public int CommentCount { get; set; }
        public int? SprintId { get; set; }
        public string SprintName { get; set; }
        public int? AssignedBy { get; set; }
        public string AssignedByFullname { get; set; }
        public string AssignedByPicture { get; set; }
        public DateTime EndDate { get; set; }
        public List<LabelDTO> Labels { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public int ReporterId { get; set; }
        public string ReporterFullname { get; set; }
        public string ReporterPicture { get; set; }
    }

    public class TaskAssignmentDTO
    {
        public int AccountId { get; set; }
        public string AccountFullname { get; set; }
        public string AccountPicture { get; set; }
    }

    public class LabelDTO
    {
        public string Name { get; set; }
    }

}
