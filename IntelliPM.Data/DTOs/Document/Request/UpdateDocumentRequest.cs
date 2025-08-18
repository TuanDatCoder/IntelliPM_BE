using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Document.Request
{
    public class UpdateDocumentRequest
    {
        [DynamicMaxLength("document_title_length")]
        [DynamicMinLength("document_title_length")]
        public string Title { get; set; }
        public string? Content { get; set; }

        [DynamicCategoryValidation("document_visibility_type", Required = true)]
        public string Visibility { get; set; }



    }
}
