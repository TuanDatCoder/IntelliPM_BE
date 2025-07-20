using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.ProjectMetric.Response
{
    public class ProjectHealthDTO
    {
        public string TimeStatus { get; set; }          // "% ahead" hoặc "% behind"
        public int TasksToBeCompleted { get; set; }     // Tổng task chưa hoàn thành
        public int OverdueTasks { get; set; }           // Task quá hạn
        public double ProgressPercent { get; set; }     // % tổng tiến độ
        public decimal CostStatus { get; set; }         // So sánh EV / AC
        public NewProjectMetricResponseDTO Cost { get; set; } = new();
    }
}
