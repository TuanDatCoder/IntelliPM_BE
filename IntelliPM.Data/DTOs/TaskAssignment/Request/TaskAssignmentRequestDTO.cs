using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskAssignment.Request
{
    public class TaskAssignmentRequestDTO
    {
        [Required(ErrorMessage = "Task ID is required")]
        [MaxLength(50, ErrorMessage = "Task ID cannot exceed 50 characters")]
        public string TaskId { get; set; }

        [Required(ErrorMessage = "Account ID is required")]
        public int AccountId { get; set; }

        [MaxLength(50, ErrorMessage = "Status cannot exceed 50 characters")]
        public string? Status { get; set; }

        [Column(TypeName = "decimal(10, 2)")]
        public decimal? PlannedHours { get; set; }


        [Column(TypeName = "decimal(10, 2)")]
        public decimal? ActualHours { get; set; }

    }
}
