using IntelliPM.Common.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.DocumentPermission
{
    public class UpdatePermissionTypeRequest
    {

        public int DocumentId { get; set; }

        [DynamicCategoryValidation("document_permission_type")]
        public string PermissionType { get; set; } = null!;
    }
}
