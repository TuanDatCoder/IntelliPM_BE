using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Sprint.Request
{
    public class SprintQuickRequestDTO
    {

        [Required(ErrorMessage = "Project ID is required")]
        public string projectKey { get; set; }

    }
}
