using IntelliPM.Common.Attributes;

namespace IntelliPM.Data.DTOs.Epic.Request
{
    public class EpicTaskTypeAssignedMembersRequestDTO
    {
        [DynamicMaxLength("title_length")]
        public string Title {get; set;}

        [DynamicCategoryValidation("task_type", Required = false)]
        public string? Type { get; set; }

        [DynamicMaxLength("description_length")]
        public string Description { get; set; }

        [DynamicDuration("task_duration_days")]
        public DateTime StartDate { get; set; }

        [DynamicDuration("task_duration_days")]
        public DateTime EndDate { get; set; }
        public string SuggestedRole { get; set; }
        public List<TaskAssignedMembersRequestDTO> AssignedMembers { get; set; } = new List<TaskAssignedMembersRequestDTO>();


    }
}
