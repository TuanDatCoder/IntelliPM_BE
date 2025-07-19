using IntelliPM.Data.DTOs.Task.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Sprint.Response
{
    public class SprintWithTaskListResponseDTO
    {
        public List<SprintResponseDTO> Sprints { get; set; } = new List<SprintResponseDTO>();
        public List<TaskBacklogResponseDTO> Tasks { get; set; } = new List<TaskBacklogResponseDTO>();
    }
}
