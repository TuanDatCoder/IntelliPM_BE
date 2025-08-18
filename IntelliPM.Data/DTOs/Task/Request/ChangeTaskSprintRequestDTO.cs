using System.ComponentModel.DataAnnotations;

namespace IntelliPM.Data.DTOs.Task.Request
{
    public class ChangeTaskSprintRequestDTO
    {
        [Required(ErrorMessage = "SprintId is required")]
        public int SprintId { get; set; }

        [Required(ErrorMessage = "CreatedBy is required")]
        public int CreatedBy { get; set; }
    }
}
