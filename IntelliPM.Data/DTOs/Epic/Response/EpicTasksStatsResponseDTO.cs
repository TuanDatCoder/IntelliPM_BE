using IntelliPM.Data.DTOs.Task.Response;
using System.Collections.Generic;

namespace IntelliPM.Data.DTOs.Epic.Response
{
    public class EpicTasksStatsResponseDTO
    {
        public List<TaskBacklogResponseDTO> Tasks { get; set; } = new List<TaskBacklogResponseDTO>();
        public int TotalTasks { get; set; }
        public int TotalToDoTasks { get; set; }
        public int TotalInProgressTasks { get; set; }
        public int TotalDoneTasks { get; set; }
    }
}