using IntelliPM.Common.Attributes;
using IntelliPM.Data.DTOs.ProjectPosition.Request;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMember.Request
{
    public class ProjectMemberWithPositionRequestDTO
    {
        [Required(ErrorMessage = "Account ID is required")]
        public int AccountId { get; set; }

        [Required(ErrorMessage = "Positions are required")]
        [DynamicCategoryValidation("account_position", Required = false)]
        public List<string> Positions { get; set; } = new List<string>();
    }
}
