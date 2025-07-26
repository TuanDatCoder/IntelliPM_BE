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



    }
}
