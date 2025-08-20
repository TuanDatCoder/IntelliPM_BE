//using IntelliPM.Common.Attributes;

//namespace IntelliPM.Data.DTOs.MeetingRescheduleRequest.Request;

//public class MeetingRescheduleRequestDTO
//{
//    public int MeetingId { get; set; }
//    public int RequesterId { get; set; }
//    public DateTime RequestedDate { get; set; }
//    public string? Reason { get; set; }

//    public int? PmId { get; set; }
//    public DateTime? PmProposedDate { get; set; }
//    public string? PmNote { get; set; }


//    [DynamicCategoryValidation("meetingReschedule_status", Required = false)]
//    public string? Status { get; set; }
//}


using IntelliPM.Common.Attributes;

namespace IntelliPM.Data.DTOs.MeetingRescheduleRequest.Request
{
    public class MeetingRescheduleRequestDTO
    {
        public int MeetingId { get; set; }
        public int RequesterId { get; set; }

   
        public DateTime RequestedDate { get; set; }

        [DynamicMaxLength("meetingReschedule_reason")]
        [DynamicMinLength("meetingReschedule_reason")]
        public string? Reason { get; set; }

        public int? PmId { get; set; }

      
        public DateTime? PmProposedDate { get; set; }

        [DynamicMaxLength("meetingReschedule_pmNote")]
        public string? PmNote { get; set; }

        [DynamicCategoryValidation("meetingReschedule_status", Required = false)]
        public string? Status { get; set; }
    }
}
