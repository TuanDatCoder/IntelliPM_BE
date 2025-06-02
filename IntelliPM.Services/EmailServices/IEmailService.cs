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


    }
}
