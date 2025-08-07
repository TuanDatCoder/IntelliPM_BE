using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.External.TaskApi
{
    public class TaskApiResponse
    {
        public bool IsSuccess { get; set; }
        public int Code { get; set; }
        public List<TaskDto> Data { get; set; } = new();
        public string Message { get; set; } = "";
    }
}
