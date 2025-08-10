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


        public async Task SendProjectCreationNotification(string pmFullName, string pmEmail, string creatorFullName, string creatorUsername, string projectName, string projectKey, int projectId, string projectDetailsUrl)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(pmEmail));
            email.Subject = $"[IntelliPM] New Project Requires Your Review: {projectName} ({projectKey})";

            var logoUrl = "https://drive.google.com/uc?export=view&id=1Z-N8gT9PspL2EGvMq_X0DDS8lFSOgBT1";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>New Project Notification - IntelliPM</title>
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
      <h1>New Project Awaiting Your Review</h1>
      <p>Dear <strong>{pmFullName}</strong>,</p>
      <p>I would like to inform you that I, <strong>{creatorFullName}</strong> ({creatorUsername}), have initiated a new project entitled <strong>{projectName}</strong> (Project Key: <strong>{projectKey}</strong>, ID: {projectId}).</p>
      <p>This project is currently awaiting your review and approval as the assigned Project Manager.</p>
      <p>Please click the button below to access full project details and proceed with the next steps, including assigning responsibilities, reviewing timelines, and confirming objectives:</p>
      <a href='{projectDetailsUrl}' class='btn'>Review Project Details</a>
      <p>If you require any additional information or support, please feel free to contact me directly.</p>
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


        public async Task SendProjectReject(string leaderFullName, string leaderEmail, string creatorFullName, string creatorUsername, string projectName, string projectKey, int projectId, string projectDetailsUrl, string reason)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(leaderEmail));
            email.Subject = $"[IntelliPM] Project Rejected: {projectName} ({projectKey})";

            var logoUrl = "https://drive.google.com/uc?export=view&id=1Z-N8gT9PspL2EGvMq_X0DDS8lFSOgBT1";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Project Rejection Notification - IntelliPM</title>
  <style>
    body {{ font-family: 'Segoe UI', sans-serif; background-color: #f9fafb; margin: 0; padding: 32px 16px; }}
    .container {{ max-width: 600px; margin: auto; background-color: #ffffff; border-radius: 12px; box-shadow: 0 8px 24px rgba(0,0,0,0.06); overflow: hidden; }}
    .top-bar {{ background-color: #ff6b6b; height: 4px; width: 100%; }}
    .content {{ padding: 32px 24px; text-align: left; }}
    .logo {{ margin-bottom: 24px; }}
    .logo img {{ width: 80px; height: auto; }}
    h1 {{ font-size: 24px; color: #1b1b1f; margin-bottom: 16px; font-weight: 700; }}
    h2 {{ font-size: 18px; color: #1b1b1f; margin: 16px 0 8px; font-weight: 600; }}
    p {{ font-size: 15px; line-height: 1.6; color: #333333; margin-bottom: 16px; }}
    .reason-box {{ background-color: #fff3f3; border-left: 4px solid #ff6b6b; padding: 16px; margin: 16px 0; border-radius: 4px; }}
    .reason-box p {{ margin: 0; font-size: 15px; color: #333333; line-height: 1.6; font-style: normal; }}
    .btn {{ display: inline-block; background: linear-gradient(to right, #ff6b6b, #e63946); color: #ffffff !important; text-decoration: none; font-weight: 600; padding: 14px 26px; border-radius: 8px; font-size: 15px; margin: 16px 0; box-shadow: 0 4px 14px rgba(255,107,107,0.3); }}
    .btn:hover {{ background: linear-gradient(to right, #e63946, #d32f2f); }}
    .footer {{ background-color: #f4f4f5; text-align: center; padding: 20px; font-size: 13px; color: #777; border-top: 1px solid #ddd; }}
    .footer p {{ margin: 4px 0; }}
    .footer a {{ color: #ff6b6b; text-decoration: none; }}
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
      <h1>Project Rejection Notification</h1>
      <p>Dear <strong>{leaderFullName}</strong>,</p>
      <p>We regret to inform you that the project <strong>{projectName}</strong> (Project Key: <strong>{projectKey}</strong>, ID: {projectId}) has been rejected by <strong>{creatorFullName}</strong> ({creatorUsername}).</p>
      <h2>Reason for Rejection</h2>
      <div class='reason-box'>
        <p>{reason}</p>
      </div>
      <p>As the Team Leader, you can review the project details to understand the context of this decision:</p>
      <a href='{projectDetailsUrl}' class='btn'>View Project Details</a>
      <p>If you have any questions or need further clarification, please contact <strong>{creatorFullName}</strong> directly.</p>
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


        public async Task SendTeamInvitation(string memberFullName, string memberEmail, string creatorFullName, string creatorUsername, string projectName, string projectKey, int projectId, string projectDetailsUrl)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(memberEmail));
            email.Subject = $"[IntelliPM] - You’ve been invited to {projectName} ({projectKey})";

            var logoUrl = "https://drive.google.com/uc?export=view&id=1Z-N8gT9PspL2EGvMq_X0DDS8lFSOgBT1";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <meta name='viewport' content='width=device-width, initial-scale=1.0'>
  <title>Project Invitation - IntelliPM</title>
  <style>
    body {{ font-family: 'Segoe UI', sans-serif; background-color: #ffffff; margin: 0; padding: 32px 16px; }}
    .container {{ max-width: 640px; margin: auto; border: 1px solid #e5e7eb; border-radius: 12px; padding: 32px 24px; text-align: center; }}
    .logo img {{ width: 80px; margin-bottom: 24px; }}
    h2 {{ font-size: 22px; color: #1b1b1f; margin: 12px 0 24px; }}
    .btn {{
        display: inline-block;
        background-color: #1b6fff;
        color: #ffffff !important;
        text-decoration: none;
        font-weight: 600;
        padding: 14px 28px;
        border-radius: 6px;
        font-size: 15px;
        margin-top: 16px;
        box-shadow: 0 4px 14px rgba(27,111,255,0.25);
    }}
    .btn:hover {{ background-color: #155ed6; }}
    .info {{ font-size: 15px; color: #444; margin-top: 32px; }}
    .footer {{
        font-size: 13px;
        color: #777;
        margin-top: 40px;
        border-top: 1px solid #ddd;
        padding-top: 20px;
    }}
    .footer a {{ color: #1b6fff; text-decoration: none; }}
    .footer a:hover {{ text-decoration: underline; }}
  </style>
</head>
<body>
  <div class='container'>
    <div class='logo'>
      <img src='{logoUrl}' alt='IntelliPM Logo'>
    </div>
    <h2>{creatorFullName} ({creatorUsername}) has invited you to join<br><strong>{projectName}</strong> ({projectKey})</h2>

    <a href='{projectDetailsUrl}' class='btn'>Join the Project</a>

    <div class='info'>
      <p>Hello <strong>{memberFullName}</strong>,</p>
      <p>You have been invited to participate in the above project on IntelliPM. Please click the button above to view the project details and confirm your involvement.</p>
      <p>If you have any questions, feel free to contact <strong>{creatorFullName}</strong> or the IntelliPM support team.</p>
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


        //public async Task SendMeetingInvitation(string toEmail, string fullName, string meetingTopic, DateTime startTime, string meetingUrl)
        //{
        //    try
        //    {
        //        // Chuyển startTime sang giờ Việt Nam và định dạng AM/PM
        //        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Windows
        //        var localStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime.ToUniversalTime(), vietnamTimeZone);

        //        var email = new MimeMessage();
        //        email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
        //        email.To.Add(MailboxAddress.Parse(toEmail));
        //        email.Subject = $"[IntelliPM] Invitation: {meetingTopic}";

        //        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        //        {
        //            Text = $@"
        //        <h2>Hi {fullName},</h2>
        //        <p>You have been invited to the meeting <b>'{meetingTopic}'</b> scheduled at <b>{localStartTime:hh:mm tt dd/MM/yyyy}</b>.</p>
        //        <p>Meeting link: <a href='{meetingUrl}'>{meetingUrl}</a></p>
        //        <p>Please confirm your attendance.</p>
        //        <br/>
        //        <p>IntelliPM Team</p>"
        //        };

        //        // Log chi tiết email
        //        Console.WriteLine("=== Email Sent ===");
        //        Console.WriteLine($"To: {toEmail}");
        //        Console.WriteLine($"Subject: {email.Subject}");
        //        Console.WriteLine("Body:");
        //        Console.WriteLine(email.Body.ToString());
        //        Console.WriteLine("==================");

        //        using var smtp = new SmtpClient();
        //        await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, SecureSocketOptions.StartTls);
        //        await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
        //        await smtp.SendAsync(email);
        //        await smtp.DisconnectAsync(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[EmailError] Failed to send invitation to {toEmail}: {ex.Message}");
        //        if (ex.InnerException != null)
        //        {
        //            Console.WriteLine($"[EmailError] Inner exception: {ex.InnerException.Message}");
        //        }
        //    }
        //}

        public async Task SendMeetingInvitation(string toEmail, string fullName, string meetingTopic, DateTime startTime, string meetingUrl)
        {
            try
            {
                // Chuyển startTime sang giờ Việt Nam và định dạng AM/PM
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time"); // Windows
                var localStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime.ToUniversalTime(), vietnamTimeZone);

                var email = new MimeMessage();
                // Tùy chỉnh phần "From" để hiển thị tên đẹp hơn
                email.From.Add(new MailboxAddress("IntelliPM Team", _config["SmtpSettings:Username"])); // Tên + email người gửi

                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = $"[IntelliPM] Invitation: {meetingTopic}";

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = $@"
        <html>
        <head>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    color: #333;
                }}
                h2 {{
                    color: #2c3e50;
                }}
                p {{
                    font-size: 14px;
                }}
                a {{
                    color: #2980b9;
                    text-decoration: none;
                }}
            </style>
        </head>
        <body>
            <h2>Hi {fullName},</h2>
            <p>You have been invited to the meeting <b>'{meetingTopic}'</b> scheduled at <b>{localStartTime:hh:mm tt dd/MM/yyyy}</b>.</p>
            <p>Meeting link: <a href='{meetingUrl}'>{meetingUrl}</a></p>
            <p>Please confirm your attendance.</p>
            <br/>
            <p>IntelliPM Team</p>
        </body>
        </html>"
                };

                // Log chi tiết email
                Console.WriteLine("=== Email Sent ===");
                Console.WriteLine($"To: {toEmail}");
                Console.WriteLine($"Subject: {email.Subject}");
                Console.WriteLine("Body:");
                Console.WriteLine(email.Body.ToString());
                Console.WriteLine("==================");

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailError] Failed to send invitation to {toEmail}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[EmailError] Inner exception: {ex.InnerException.Message}");
                }
            }
        }

        //public async Task SendMeetingCancellationEmail(string toEmail, string fullName, string meetingTopic, DateTime startTime, string meetingUrl)
        //{
        //    try
        //    {
        //        var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
        //        var localStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime.ToUniversalTime(), vietnamTimeZone);

        //        var email = new MimeMessage();
        //        email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
        //        email.To.Add(MailboxAddress.Parse(toEmail));
        //        email.Subject = "📢 Cuộc họp đã bị hủy";

        //        email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
        //        {
        //            Text = $@"
        //    <h2>Xin chào {fullName},</h2>
        //    <p>Buổi họp với tiêu đề <b>'{meetingTopic}'</b> dự kiến diễn ra vào <b>{localStartTime:hh:mm tt dd/MM/yyyy}</b> đã bị <span style='color:red;'><b>hủy bỏ</b></span>.</p>
        //    <p>Nếu bạn có bất kỳ thắc mắc nào, vui lòng liên hệ lại với ban tổ chức.</p>
        //    <br/>
        //    <p>Trân trọng,</p>
        //    <p><b>IntelliPM Team</b></p>"
        //        };

        //        Console.WriteLine("=== Email Cancel Sent ===");
        //        Console.WriteLine($"To: {toEmail}");
        //        Console.WriteLine($"Subject: {email.Subject}");
        //        Console.WriteLine("Body:");
        //        Console.WriteLine(email.Body.ToString());
        //        Console.WriteLine("=========================");

        //        using var smtp = new SmtpClient();
        //        await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, SecureSocketOptions.StartTls);
        //        await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
        //        await smtp.SendAsync(email);
        //        await smtp.DisconnectAsync(true);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"[EmailError] Failed to send cancellation email to {toEmail}: {ex.Message}");
        //        if (ex.InnerException != null)
        //        {
        //            Console.WriteLine($"[EmailError] Inner exception: {ex.InnerException.Message}");
        //        }
        //    }
        //}
        public async Task SendMeetingCancellationEmail(string toEmail, string fullName, string meetingTopic, DateTime startTime, string meetingUrl)
        {
            try
            {
                var vietnamTimeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");
                var localStartTime = TimeZoneInfo.ConvertTimeFromUtc(startTime.ToUniversalTime(), vietnamTimeZone);

                var email = new MimeMessage();
                // Cập nhật phần "From" cho trang trọng hơn
                email.From.Add(new MailboxAddress("IntelliPM Team", _config["SmtpSettings:Username"])); // Tên + Email người gửi

                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = "[IntelliPM] Meeting Cancellation Notice";

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = $@"
            <html>
            <head>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        color: #333;
                    }}
                    h2 {{
                        color: #2c3e50;
                    }}
                    p {{
                        font-size: 14px;
                    }}
                    span {{
                        color: red;
                    }}
                    a {{
                        color: #2980b9;
                        text-decoration: none;
                    }}
                </style>
            </head>
            <body>
                <h2>Dear {fullName},</h2>
                <p>We regret to inform you that the meeting titled <b>'{meetingTopic}'</b>, scheduled for <b>{localStartTime:hh:mm tt dd/MM/yyyy}</b>, has been <span><b>canceled</b></span>.</p>
                <p>If you have any questions or concerns, please feel free to reach out to the organizing team.</p>
                <br/>
                <p>Best regards,</p>
                <p><b>IntelliPM Team</b></p>
            </body>
            </html>"
                };

                Console.WriteLine("=== Email Cancellation Sent ===");
                Console.WriteLine($"To: {toEmail}");
                Console.WriteLine($"Subject: {email.Subject}");
                Console.WriteLine("Body:");
                Console.WriteLine(email.Body.ToString());
                Console.WriteLine("=========================");

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailError] Failed to send cancellation email to {toEmail}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[EmailError] Inner exception: {ex.InnerException.Message}");
                }
            }
        }


        public async Task SendShareDocumentEmail(string toEmail, string documentTitle, string message, string link)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = $"[IntelliPM] 📄 {documentTitle} has been shared with you";
            var logoUrl = "https://drive.google.com/uc?export=view&id=1Z-N8gT9PspL2EGvMq_X0DDS8lFSOgBT1";
            var htmlBody = $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8' />
  <title>Document Invitation</title>
</head>
<body style='background-color: #f9f9f9; padding: 50px 0; font-family: Arial, sans-serif;'>
  <div style='max-width: 600px; margin: auto; background-color: #ffffff; padding: 40px; border-radius: 12px; box-shadow: 0 4px 16px rgba(0,0,0,0.05); text-align: center;'>
    
    <img src='{logoUrl}' alt='IntelliPM Logo' style='max-height: 40px; margin-bottom: 30px;' />

    <h2 style='font-size: 20px; color: #333; margin-bottom: 10px;'>
      You've been invited to collaborate on
    </h2>

    <h1 style='font-size: 24px; color: #00C875; margin-bottom: 20px;'>
      ""{documentTitle}""
    </h1>

    <p style='color: #555; font-size: 16px; line-height: 1.6; margin-bottom: 30px;'>
      {message}
    </p>

    <a href='{link}' target='_blank' style='
      background-color: #00C875;
      color: white;
      text-decoration: none;
      font-weight: bold;
      padding: 12px 30px;
      border-radius: 6px;
      display: inline-block;
      font-size: 16px;
    '>
      Accept Invitation
    </a>

    <p style='margin-top: 40px; font-size: 12px; color: #888;'>
      If you didn’t expect this invitation, feel free to ignore this email.
    </p>

    <p style='font-size: 12px; color: #aaa; margin-top: 10px;'>
      Sent via <strong>IntelliPM</strong> – your smart project management tool
    </p>
  </div>
</body>
</html>";


            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = htmlBody
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }


        public async Task SendMeetingReminderEmail(string toEmail, string fullName, string meetingTopic, DateTime startTime, string meetingUrl)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = $"[IntelliPM] Reminder: Upcoming Meeting '{meetingTopic}'";

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = $@"
                <h2>Hi {fullName},</h2>
                <p>This is a reminder that the meeting <b>'{meetingTopic}'</b> will start at <b>{startTime:HH:mm dd/MM/yyyy}</b> (in 30 minutes).</p>
                <p>Meeting link: <a href='{meetingUrl}'>{meetingUrl}</a></p>
                <p>Please be prepared and join on time.</p>
                <br/>
                <p>IntelliPM Team</p>"
                };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailError] Failed to send reminder to {toEmail}: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[EmailError] Inner exception: {ex.InnerException.Message}");
                }
            }
        }
        public async Task SendEmailTeamLeader(List<string> emails, string message)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));

            foreach (var emailAddress in emails)
            {
                email.To.Add(MailboxAddress.Parse(emailAddress));
            }

            email.Subject = $"[IntelliPM] New Document Created";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
  <meta charset='UTF-8'>
  <title>New Document Request</title>
</head>
<body style='font-family:Segoe UI,sans-serif;background-color:#f9fafb;padding:32px'>
  <div style='max-width:600px;margin:auto;background-color:#fff;padding:32px;border-radius:12px;box-shadow:0 6px 18px rgba(0,0,0,0.06)'>
    <div style='text-align:center;margin-bottom:24px'>
      <img src='https://drive.google.com/uc?export=view&id=1Z-N8gT9PspL2EGvMq_X0DDS8lFSOgBT1' alt='IntelliPM Logo' style='width:80px'>
    </div>
    <h2 style='color:#1b1b1f'>📄 A document request has been created</h2>
    <p style='font-size:15px;color:#333'>{message}</p>
    <div style='text-align:center;margin-top:30px'>
      <a href='' target='_blank' style='display:inline-block;background-color:#1b6fff;color:#fff;padding:14px 24px;border-radius:8px;text-decoration:none;font-weight:600;font-size:15px;box-shadow:0 4px 12px rgba(27,111,255,0.25)'>View Document</a>
    </div>
    <div style='margin-top:40px;font-size:13px;color:#888;text-align:center'>
      <p>Sent via IntelliPM</p>
      <p>© 2025 IntelliPM. All rights reserved.</p>
    </div>
  </div>
</body>
</html>"
            };

            using var smtp = new SmtpClient();
            int port = int.Parse(_config["SmtpSettings:Port"]);
            await smtp.ConnectAsync(_config["SmtpSettings:Host"], port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendTaskCommentNotificationEmail(string toEmail, string fullName, string taskId, string taskTitle, string commentContent)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = $"[IntelliPM] New comment in task {taskId}: {taskTitle}";

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = $@"
        <h3>Hello {fullName},</h3>
        <p>A new comment was added on task <b>{taskId}</b>: <b>{taskTitle}</b>.</p>
        <p>Comment content:</p>
        <blockquote>{commentContent}</blockquote>
        <p>
            👉 <a href='http://localhost:5173/project/work-item-detail?taskId={taskId}'>
            View Task Detail
            </a>
        </p>
        <br/>
        <p>IntelliPM Notification System</p>"
                };
                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailError] Failed to send comment email to {toEmail}: {ex.Message}");
            }
        }

        public async Task SendSubtaskCommentNotificationEmail(string toEmail, string fullName, string subtaskId, string subtaskTitle, string commentContent)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = $"[IntelliPM] New comment in subtask {subtaskId}: {subtaskTitle}";

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = $@"
        <h3>Hello {fullName},</h3>
        <p>A new comment was added on subtask <b>{subtaskId}</b>: <b>{subtaskTitle}</b>.</p>
        <p>Comment content:</p>
        <blockquote>{commentContent}</blockquote>
        <p>
            👉 <a href='http://localhost:5173/project/child-work?subtaskId={subtaskId}'>
            View Subtask Detail
            </a>
        </p>
        <br/>
        <p>IntelliPM Notification System</p>"
                };

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailError] Failed to send comment email to {toEmail}: {ex.Message}");
            }
        }

        public async Task SendEpicCommentNotificationEmail(string toEmail, string fullName, string epicId, string epicTitle, string commentContent)
        {
            try
            {
                var email = new MimeMessage();
                email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
                email.To.Add(MailboxAddress.Parse(toEmail));
                email.Subject = $"[IntelliPM] New comment in epic: {epicId}: {epicTitle}";

                email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
                {
                    Text = $@"
        <h3>Hello {fullName},</h3>
        <p>A new comment was added on epic <b>{epicId}</b>: <b>{epicTitle}</b>.</p>
        <p>Comment content:</p>
        <blockquote>{commentContent}</blockquote>
        <p>
            👉 <a href='http://localhost:5173/project/epic?epicId={epicId}'>
            View Epic Detail
            </a>
        </p>
        <br/>
        <p>IntelliPM Notification System</p>"
                };


                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EmailError] Failed to send comment email to {toEmail}: {ex.Message}");
            }
        }

        public async Task SendTaskAssignmentEmail(string fullName, string userEmail, string taskId, string taskTitle)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("IntelliPM Team", _config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = $"[IntelliPM] You have been assigned task {taskId}: {taskTitle}";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
        <h2>Hi {fullName},</h2>
        <p>You have been assigned a new task:</p>
        <ul>
          <li><b>ID:</b> {taskId}</li>
          <li><b>Title:</b> {taskTitle}</li>
        </ul>
       
        <p>Please check the system for more details.</p>
        <br/>
        <p>Best regards,<br/>IntelliPM Notification System</p>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["SmtpSettings:Host"], int.Parse(_config["SmtpSettings:Port"]), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendSubtaskAssignmentEmail(string fullName, string userEmail, string subtaskId, string subtaskTitle)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("IntelliPM Team", _config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = $"[IntelliPM] You have been assigned subtask {subtaskId}: {subtaskTitle}";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
        <h2>Hi {fullName},</h2>
        <p>You have been assigned a new subtask:</p>
        <ul>
          <li><b>ID:</b> {subtaskId}</li>
          <li><b>Title:</b> {subtaskTitle}</li>
        </ul>
       
        <p>Please check the system for more details.</p>
        <br/>
        <p>Best regards,<br/>IntelliPM Notification System</p>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["SmtpSettings:Host"], int.Parse(_config["SmtpSettings:Port"]), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

     

        public async Task SendDocumentShareEmailMeeting(
    string toEmail,
    string subject,
    string body,
    byte[] fileBytes,
    string fileName)
        {
            var message = new MimeMessage();
            message.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                TextBody = body
            };

            // 👇 Đính kèm file
            builder.Attachments.Add(fileName, fileBytes);

            message.Body = builder.ToMessageBody();

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["SmtpSettings:Host"], 587, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
            await smtp.SendAsync(message);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendRiskAssignmentEmail(string assigneeFullName, string assigneeEmail, string riskKey, string riskTitle, string projectKey, string severityLevel, DateTime? dueDate, string riskDetailUrl)
        {
            var email = new MimeMessage();
            email.From.Add(new MailboxAddress("IntelliPM Team", _config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(assigneeEmail));
            email.Subject = $"[IntelliPM] You have been assigned risk {riskKey}: {riskTitle}";

            var logoUrl = "https://drive.google.com/uc?export=view&id=1Z-N8gT9PspL2EGvMq_X0DDS8lFSOgBT1";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <style>
        body {{
            font-family: 'Arial', sans-serif;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 20px auto;
            background-color: #ffffff;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
            overflow: hidden;
        }}
        .header {{
            background-color: #007bff;
            padding: 20px;
            text-align: center;
        }}
        .header img {{
            max-width: 150px;
        }}
        .content {{
            padding: 20px;
            color: #333333;
        }}
        .content h2 {{
            color: #007bff;
            font-size: 24px;
            margin-bottom: 10px;
        }}
        .content p {{
            font-size: 16px;
            line-height: 1.6;
            margin: 10px 0;
        }}
        .details {{
            background-color: #f9f9f9;
            padding: 15px;
            border-radius: 5px;
            margin: 15px 0;
        }}
        .details ul {{
            list-style: none;
            padding: 0;
            font-size: 14px;
        }}
        .details ul li {{
            margin-bottom: 8px;
        }}
        .details ul li strong {{
            display: inline-block;
            width: 120px;
            color: #555555;
        }}
        .button {{
            display: inline-block;
            padding: 10px 20px;
            margin: 20px 0;
            background-color: #007bff;
            color: #ffffff;
            text-decoration: none;
            border-radius: 5px;
            font-size: 16px;
        }}
        .footer {{
            background-color: #f4f4f4;
            padding: 15px;
            text-align: center;
            font-size: 12px;
            color: #777777;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='{logoUrl}' alt='IntelliPM Logo'>
        </div>
        <div class='content'>
            <h2>Hi {assigneeFullName},</h2>
            <p>You have been assigned a new risk in the IntelliPM system. Please review the details below and take appropriate action.</p>
            <div class='details'>
                <ul>
                    <li><strong>Risk ID:</strong> {riskKey}</li>
                    <li><strong>Title:</strong> {riskTitle}</li>
                    <li><strong>Project:</strong> {projectKey}</li>
                    <li><strong>Severity:</strong> {severityLevel}</li>
                    <li><strong>Due Date:</strong> {(dueDate.HasValue ? dueDate.Value.ToString("MMM dd, yyyy") : "Not specified")}</li>
                </ul>
            </div>
            <p>Please log in to the IntelliPM system to view full details and manage this risk.</p>
            <a href='{riskDetailUrl}' class='button'>View Risk Details</a>
        </div>
        <div class='footer'>
            <p>Best regards,<br>IntelliPM Notification System</p>
            <p>&copy; {DateTime.UtcNow.Year} IntelliPM. All rights reserved.</p>
        </div>
    </div>
</body>
</html>"
            };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(_config["SmtpSettings:Host"], int.Parse(_config["SmtpSettings:Port"]), SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config["SmtpSettings:Username"], _config["SmtpSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }

}
