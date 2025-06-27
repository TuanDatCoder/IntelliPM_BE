using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.WorkItemLabel.Response
{
    public class WorkItemLabelResponseDTO
    {
        public int Id { get; set; }
        public int LabelId { get; set; }
        public string? TaskId { get; set; }
        public string? EpicId { get; set; }
        public string? SubtaskId { get; set; }
        public bool IsDeleted { get; set; }
    }
}
