using Microsoft.Extensions.Configuration;
using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using MailKit.Security;

namespace IntelliPM.Services.EmailServices
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }
        public async Task SendRegistrationEmail(string fullName, string userEmail, string verificationUrl)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = "[IntelliPM] - Verify Your Account";

            var logoUrl = "https://drive.google.com/uc?export=view&id=1Z-N8gT9PspL2EGvMq_X0DDS8lFSOgBT1";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Verify Email - IntelliPM</title>
  <style>
    body {{
      font-family: 'Segoe UI', sans-serif;
      background-color: #f9fafb;
      margin: 0;
      padding: 32px 16px;
    }}
    .container {{
      max-width: 600px;
      margin: auto;
      background-color: #ffffff;
      border-radius: 12px;
      box-shadow: 0 8px 24px rgba(0,0,0,0.06);
      overflow: hidden;
    }}
    .top-bar {{
      background-color: #1b6fff;
      height: 4px;
      width: 100%;
    }}
    .content {{
      padding: 32px 24px;
      text-align: left;
    }}
    .logo {{
      margin-bottom: 24px;
    }}
    .logo img {{
      width: 80px;
      height: auto;
    }}
    h1 {{
      font-size: 24px;
      color: #1b1b1f;
      margin-bottom: 16px;
    }}
    p {{
      font-size: 15px;
      line-height: 1.6;
      color: #333333;
      margin-bottom: 18px;
    }}
    .btn {{
      display: inline-block;
      background-color: #1b6fff;
      color: #ffffff !important;
      text-decoration: none;
      font-weight: 600;
      padding: 14px 26px;
      border-radius: 8px;
      font-size: 15px;
      margin-top: 8px;
      box-shadow: 0 4px 14px rgba(27,111,255,0.3);
    }}
    .btn:hover {{
      background-color: #155ed6;
    }}
    .footer {{
      background-color: #f4f4f5;
      text-align: center;
      padding: 20px;
      font-size: 13px;
      color: #777;
      border-top: 1px solid #ddd;
    }}
    .footer p {{
      margin: 4px 0;
    }}
    .footer a {{
      color: #1b6fff;
      text-decoration: none;
    }}
    .footer a:hover {{
      text-decoration: underline;
    }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='top-bar'></div>
    <div class='content'>
      <div class='logo'>
        <img src='{logoUrl}' alt='IntelliPM Logo'>
      </div>
      <h1>Welcome to IntelliPM 👋</h1>
      <p>Hi <strong>{fullName}</strong>,</p>
      <p>Thanks for registering with <strong>IntelliPM</strong> – your AI assistant for smarter project management. We're here to help you stay productive and manage projects effortlessly.</p>
      <p>To complete your registration, please verify your email address by clicking the button below:</p>
      <a href='{verificationUrl}' class='btn'>Verify My Email</a>
      <p style='margin-top:30px;'>If you didn’t sign up for this account, you can safely ignore this email.</p>
    </div>
    <div class='footer'>
      <p>7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, Hồ Chí Minh</p>
      <p>FPT University HCMC</p>
      <p><a href='mailto:intellipm.official@gmail.com'>intellipm.official@gmail.com</a></p>
      <p>© 2025 IntelliPM. All rights reserved.</p>
    </div>
  </div>
</body>
</html>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }



        //----------------------GOOGLE--------------------------------------
        public async Task SendRegistrationEmail(string fullName, string userEmail)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = "[ConstructionEquipmentRental] - Welcome!";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Welcome Email</title>
  <style>
    body {{
      font-family: 'Segoe UI', sans-serif;
      background-color: #f9fafb;
      margin: 0;
      padding: 32px 16px;
    }}
    .container {{
      max-width: 600px;
      margin: auto;
      background-color: #ffffff;
      border-radius: 12px;
      box-shadow: 0 8px 24px rgba(0,0,0,0.06);
    }}
    .top-bar {{
      height: 4px;
      background-color: #4caf50;
    }}
    .content {{
      padding: 32px 24px;
      text-align: left;
    }}
    h1 {{
      color: #2e7d32;
      font-size: 24px;
      margin-bottom: 16px;
    }}
    p {{
      font-size: 15px;
      line-height: 1.6;
      margin-bottom: 18px;
      color: #333;
    }}
    .footer {{
      background-color: #f1f1f1;
      text-align: center;
      padding: 20px;
      font-size: 13px;
      color: #777;
      border-top: 1px solid #ddd;
    }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='top-bar'></div>
    <div class='content'>
      <h1>Welcome to ConstructionEquipmentRental!</h1>
      <p>Hi <strong>{fullName}</strong>,</p>
      <p>Thank you for registering with <strong>ConstructionEquipmentRental</strong>. We're excited to have you on board and ready to help you rent the best construction equipment with ease and confidence.</p>
      <p>We hope you enjoy your experience with us!</p>
      <p>Cheers,<br/>The ConstructionEquipmentRental Team</p>
    </div>
    <div class='footer'>
      <p>Thank you for choosing us.</p>
    </div>
  </div>
</body>
</html>"
            };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendAccountResetPassword(string fullName, string userEmail, string OTP)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = "[ConstructionEquipmentRental] - Password Reset Request";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Password Reset</title>
  <style>
    body {{
      font-family: 'Segoe UI', sans-serif;
      background-color: #f9fafb;
      margin: 0;
      padding: 32px 16px;
    }}
    .container {{
      max-width: 600px;
      margin: auto;
      background-color: #ffffff;
      border-radius: 12px;
      box-shadow: 0 8px 24px rgba(0,0,0,0.06);
    }}
    .top-bar {{
      height: 4px;
      background-color: #4caf50;
    }}
    .content {{
      padding: 32px 24px;
      text-align: left;
    }}
    h1 {{
      font-size: 22px;
      color: #2e7d32;
      margin-bottom: 16px;
    }}
    p {{
      font-size: 15px;
      line-height: 1.6;
      color: #333;
      margin-bottom: 18px;
    }}
    .otp {{
      font-size: 24px;
      font-weight: bold;
      color: #4caf50;
      letter-spacing: 4px;
    }}
    .footer {{
      background-color: #f1f1f1;
      text-align: center;
      padding: 20px;
      font-size: 13px;
      color: #777;
      border-top: 1px solid #ddd;
    }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='top-bar'></div>
    <div class='content'>
      <h1>Password Reset Request</h1>
      <p>Hi <strong>{fullName}</strong>,</p>
      <p>You’ve requested to reset your password for <strong>ConstructionEquipmentRental</strong>. Please use the OTP below to proceed:</p>
      <p class='otp'>{OTP}</p>
      <p>This OTP is valid for a limited time. If you didn’t request this action, please ignore this email.</p>
      <p>Thank you,<br/>The ConstructionEquipmentRental Team</p>
    </div>
    <div class='footer'>
      <p>Thank you for trusting us.</p>
    </div>
  </div>
</body>
</html>"
            };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }


        public async Task SendProjectCreationNotification(string pmFullName, string pmEmail, string creatorFullName, string creatorUsername, int projectId, string projectDetailsUrl)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(pmEmail));
            email.Subject = "[IntelliPM] - Project Creation Notification";

            var logoUrl = "https://drive.google.com/uc?export=view&id=1Z-N8gT9PspL2EGvMq_X0DDS8lFSOgBT1";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Project Creation Notification - IntelliPM</title>
  <style>
    body {{ font-family: 'Segoe UI', sans-serif; background-color: #f9fafb; margin: 0; padding: 32px 16px; }}
    .container {{ max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 8px 24px rgba(0,0,0,0.06); overflow: hidden; }}
    .top-bar {{ background-color: #1b6fff; height: 4px; width: 100%; }}
    .content {{ padding: 32px 24px; text-align: left; }}
    .logo {{ margin-bottom: 24px; }}
    .logo img {{ width: 80px; height: auto; }}
    h1 {{ font-size: 24px; color: #1b1b1f; margin-bottom: 16px; }}
    p {{ font-size: 15px; line-height: 1.6; color: #333333; margin-bottom: 18px; }}
    .btn {{ display: inline-block; background-color: #1b6fff; color: #ffffff !important; text-decoration: none; font-weight: 600; padding: 14px 26px; border-radius: 8px; font-size: 15px; margin-top: 8px; box-shadow: 0 4px 14px rgba(27,111,255,0.3); }}
    .btn:hover {{ background-color: #155ed6; }}
    .footer {{ background-color: #f4f4f5; text-align: center; padding: 20px; font-size: 13px; color: #777; border-top: 1px solid #ddd; }}
    .footer p {{ margin: 4px 0; }}
    .footer a {{ color: #1b6fff; text-decoration: none; }}
    .footer a:hover {{ text-decoration: underline; }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='top-bar'></div>
    <div class='content'>
      <div class='logo'>
        <img src='{logoUrl}' alt='IntelliPM Logo'>
      </div>
      <h1>Project Creation Notification 👋</h1>
      <p>Hi <strong>{pmFullName}</strong>,</p>
      <p>A new project member, <strong>{creatorFullName}</strong> ({creatorUsername}), has created the entire project with ID {projectId}. Please review the details below:</p>
      <p>To view the project details, click the button below:</p>
      <a href='{projectDetailsUrl}' class='btn'>View Project Details</a>
      <p>This member is now part of the project team. Kindly take necessary actions to manage the project effectively.</p>
      <p>If you didn’t expect this action, please contact support.</p>
    </div>
    <div class='footer'>
      <p>7 Đ. D1, Long Thạnh Mỹ, Thủ Đức, Hồ Chí Minh</p>
      <p>FPT University HCMC</p>
      <p><a href='mailto:intellipm.official@gmail.com'>intellipm.official@gmail.com</a></p>
      <p>© 2025 IntelliPM. All rights reserved.</p>
    </div>
  </div>
</body>
</html>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }



    }
}
