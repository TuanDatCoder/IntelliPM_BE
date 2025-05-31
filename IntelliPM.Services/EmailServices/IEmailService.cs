using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IntelliPM.Services.EmailServices
{
    public interface IEmailService 
    {
        Task SendAccountResetPassword(string fullName, string userEmail, string OTP);
        Task SendRegistrationEmail(string fullName, string userEmail);
        Task SendRegistrationEmail(string fullName, string userEmail, string verificationUrl);
        Task SendStoreCreationEmail(string fullName, string userEmail, string storeName);
        Task SendApprovalEmail(string fullName, string userEmail, string storeName, string adminName);
        Task SendRejectionEmail(string fullName, string userEmail, string storeName, string adminName, string reason);

    }
}
