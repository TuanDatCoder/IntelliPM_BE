using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Task.Request
{
    public class ChangeTaskEpicRequestDTO
    {
        public string? EpicId { get; set; }
        public int CreatedBy { get; set; }
    }
}
