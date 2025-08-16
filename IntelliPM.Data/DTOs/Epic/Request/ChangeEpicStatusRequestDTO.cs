using IntelliPM.Common.Attributes;
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

        [DynamicCategoryValidation("epic_status", Required = false)]
        public string? Status { get; set; }
    }
}
