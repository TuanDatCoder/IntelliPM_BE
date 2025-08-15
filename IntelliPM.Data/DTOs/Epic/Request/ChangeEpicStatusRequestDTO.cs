using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Epic.Request
{
    public class ChangeEpicStatusRequestDTO
    {
        public int CreatedBy { get; set; }
        public string? Status { get; set; }
    }
}
