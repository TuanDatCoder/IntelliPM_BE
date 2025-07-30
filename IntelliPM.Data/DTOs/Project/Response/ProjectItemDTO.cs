using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.Project.Response
{
    public class ProjectItemDTO
    {
        public string Id { get; set; }
        public string Name { get; set; } = null!;
        public string Type { get; set; } = null!; 
    }
}
