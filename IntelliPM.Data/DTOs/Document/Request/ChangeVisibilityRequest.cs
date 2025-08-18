using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Document.Request
{
    public class ChangeVisibilityRequest
    {

        [Required]
        [DynamicCategoryValidation("document_visibility_type", Required = true)]
        public string Visibility { get; set; } = string.Empty;
    }
}
