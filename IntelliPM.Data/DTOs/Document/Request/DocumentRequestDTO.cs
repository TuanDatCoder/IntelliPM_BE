using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Document.Request
{
    public class DocumentRequestDTO
    {
        [Required]
        public int ProjectId { get; set; }
        public string? TaskId { get; set; }
        public string? EpicId { get; set; }
        public string? SubTaskId { get; set; }

        [DynamicMaxLength("document_title_length")]
        [DynamicMinLength("document_title_length")]
        public string Title { get; set; }

        public string? Content { get; set; }

        [Required]
        [DynamicCategoryValidation("document_visibility_type", Required = true)]
        public string Visibility { get; set; } 



    }
}
