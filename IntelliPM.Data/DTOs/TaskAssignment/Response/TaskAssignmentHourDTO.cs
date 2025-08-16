using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Data.DTOs.TaskAssignment.Response
{
    public class TaskAssignmentHourDTO
    {
        public int Id { get; set; }
        public string TaskId { get; set; }
        public int AccountId { get; set; }
        public decimal? HourlyRate { get; set; }
        public decimal? WorkingHoursPerDay { get; set; }
        public string? AccountFullname { get; set; }
        public string? AccountUsername { get; set; }
        public string? AccountPicture { get; set; }
        public decimal? PlannedHours { get; set; }
        public decimal? ActualHours { get; set; }
    }
}
