using IntelliPM.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskCheckList.Request
{
    public class SubtaskRequestDTO
    {
        public int AssignedBy { get; set; }

        public string Title { get; set; } = null!;

        public string? Description { get; set; }
    }
}
