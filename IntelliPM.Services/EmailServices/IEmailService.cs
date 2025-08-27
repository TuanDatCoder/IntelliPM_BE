using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.EmailServices
{
    public interface IEmailService
    {
        Task SendRegistrationEmail(string fullName, string userEmail, string verificationUrl);
        Task SendRegistrationEmail(string fullName, string userEmail);
        Task SendAccountResetPassword(string fullName, string userEmail, string OTP);
        Task SendProjectCreationNotification(string pmFullName, string pmEmail, string creatorFullName, string creatorUsername, string projectName, string projectKey, int projectId, string projectDetailsUrl);

        Task SendProjectReject(string leaderFullName, string leaderEmail, string creatorFullName, string creatorUsername, string projectName, string projectKey, int projectId, string projectDetailsUrl, string reason);

        Task SendTeamInvitation(string memberFullName, string memberEmail, string creatorFullName, string creatorUsername, string projectName, string projectKey, int projectId, string projectDetailsUrl);

        Task SendMeetingCancellationEmail(string toEmail, string fullName, string meetingTopic, DateTime startTime, string meetingUrl);


        Task SendMeetingInvitation(string toEmail, string fullName, string meetingTopic, DateTime startTime, string meetingUrl);

        Task SendShareDocumentEmail(string toEmail, string documentTitle, string message, string link);

        Task SendEmailTeamLeader(List<string> email, string message);

        Task SendMeetingReminderEmail(string toEmail, string fullName, string meetingTopic, DateTime startTime, string meetingUrl);

        Task SendTaskCommentNotificationEmail(string toEmail, string fullName, string taskId, string taskTitle, string commentContent);

        Task SendSubtaskCommentNotificationEmail(string toEmail, string fullName, string subtaskId, string subtaskTitle, string commentContent);

        Task SendEpicCommentNotificationEmail(string toEmail, string fullName, string epicId, string epicTitle, string commentContent);

        Task SendTaskAssignmentEmail(string assigneeFullName, string assigneeEmail, string taskId, string taskTitle);
        Task SendDocumentShareEmailMeeting(string toEmail, string subject, string body, byte[] fileBytes, string fileName);

        Task SendSubtaskAssignmentEmail(string assigneeFullName, string assigneeEmail, string subtaskId, string subtaskTitle);

        Task SendEpicAssignmentEmail(string assigneeFullName, string assigneeEmail, string epicId, string epicName);

        Task SendRiskAssignmentEmail(string assigneeFullName, string assigneeEmail, string riskKey, string riskTitle, string projectKey, string severityLevel, DateTime? dueDate, string riskDetailUrl);

        Task SendOverdueTaskNotificationEmailAsync(string assigneeFullName, string assigneeEmail, string taskId, string taskTitle, string projectKey, DateTime plannedEndDate, string taskDetailUrl);
        Task SendMeetingUpdateEmail(string toEmail, string fullName, string meetingTopic, DateTime startTime, string meetingUrl, string changeSummaryHtml);
        Task SendMeetingRemovalEmail(string toEmail, string fullName, string meetingTopic, DateTime startTime, string meetingUrl);
        Task SendOverdueRiskNotificationEmailAsync(string assigneeFullName, string assigneeEmail, string riskKey, string riskTitle, string projectKey, DateTime dueDate, string riskDetailUrl);
    }
}
