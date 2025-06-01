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

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Verify Your IntelliPM Account</title>
    <style>
        body {{
            font-family: 'Segoe UI', sans-serif;
            background-color: #f4f6fa;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 40px auto;
            background-color: #ffffff;
            border-radius: 12px;
            box-shadow: 0 8px 20px rgba(0,0,0,0.05);
            overflow: hidden;
        }}
        .header {{
            background-color: #4A3AFF;
            color: #ffffff;
            padding: 30px 20px;
            text-align: center;
        }}
        .header h1 {{
            margin: 0;
            font-size: 26px;
        }}
        .header p {{
            font-size: 15px;
            opacity: 0.9;
        }}
        .content {{
            padding: 30px 20px;
            color: #333333;
        }}
        .content p {{
            font-size: 16px;
            line-height: 1.6;
            margin-bottom: 20px;
        }}
        .btn {{
            display: inline-block;
            background-color: #4A3AFF;
            color: #ffffff !important;
            padding: 14px 28px;
            text-decoration: none;
            border-radius: 10px;
            font-size: 16px;
            font-weight: 600;
            box-shadow: 0 4px 14px rgba(74, 58, 255, 0.4);
            transition: all 0.3s ease;
        }}
        .btn:hover {{
            background-color: #3f32cc;
            transform: translateY(-2px);
            box-shadow: 0 8px 24px rgba(74, 58, 255, 0.5);
        }}
        .footer {{
            background-color: #f0f0f0;
            text-align: center;
            padding: 18px;
            font-size: 13px;
            color: #777777;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h1>Welcome to IntelliPM 👋</h1>
            <p>Your AI Assistant for Smarter Project Management</p>
        </div>
        <div class='content'>
            <p>Hi <strong>{fullName}</strong>,</p>
            <p>Thanks for registering with <strong>IntelliPM</strong>! We're excited to help you simplify project management through AI and smart automation.</p>
            <p>To complete your registration, please confirm your email by clicking the button below:</p>
            <a href='{verificationUrl}' class='btn'>Verify My Email</a>
            <p style='margin-top:30px;'>If you didn’t sign up, you can safely ignore this email.</p>
        </div>
        <div class='footer'>
            &copy; 2025 IntelliPM Team – All rights reserved.
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

        

    }
}
