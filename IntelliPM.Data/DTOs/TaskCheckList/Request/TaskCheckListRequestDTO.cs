using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskCheckList.Request
{
    public class TaskCheckListRequestDTO
    {
        //public int Id { get; set; }

        public string TaskId { get; set; }

        public string Title { get; set; } = null!;

        //public string? Status { get; set; }

        public bool ManualInput { get; set; }

        public bool GenerationAiInput { get; set; }
    }
}
