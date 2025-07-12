using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMember.Request
{
    public class ProjectMemberNoProjectIdRequestDTO
    {
        [Required(ErrorMessage = "Account ID is required")]
        public int AccountId { get; set; }

    }
}
