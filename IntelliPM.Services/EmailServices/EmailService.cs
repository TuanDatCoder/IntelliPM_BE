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


        public async Task SendRegistrationEmail(string fullName, string userEmail)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("SmtpSettings:Username").Value));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = "[ConstructionEquipmentRental Application] - Welcome to ConstructionEquipmentRental!";
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
        <!DOCTYPE html>
        <html lang='en'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>Welcome to ConstructionEquipmentRental</title>
            <style>
                body {{
                    font-family: Arial, sans-serif;
                    background-color: #f9f9f9;
                    color: #333;
                    margin: 0;
                    padding: 0;
                }}
                .container {{
                    max-width: 700px;
                    margin: 0 auto;
                    padding: 20px;
                    background-color: #ffffff;
                    border-radius: 10px;
                    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
                }}
                .header {{
                    background-color: #4caf50; /* Màu xanh nhẹ */
                    color: #fff;
                    padding: 20px;
                    text-align: center;
                    border-radius: 8px 8px 0 0;
                    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2);
                }}
                .header h1 {{
                    font-size: 36px;
                    font-weight: bold;
                    margin: 0;
                }}
                .body {{
                    padding: 20px;
                    background-color: #ffffff;
                    border-radius: 0 0 8px 8px;
                    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
                }}
                .body p {{
                    font-size: 16px;
                    line-height: 1.5;
                    color: #333;
                }}
                .footer {{
                    padding: 10px;
                    text-align: center;
                    font-size: 14px;
                    color: #888;
                    background-color: #e0e0e0; /* Màu xám nhẹ */
                    border-top: 2px solid #bbb;
                    border-radius: 0 0 8px 8px;
                    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>Welcome to ConstructionEquipmentRental!</h1>
                </div>
                <div class='body'>
                    <p>Hi {fullName},</p>
                    <p>Thank you for registering with ConstructionEquipmentRental. We're excited to have you on board and ready to help you rent the best construction equipment!</p>
                    <p>We hope you enjoy the experience!</p>
                    <p>Thank you,</p>
                    <p>The ConstructionEquipmentRental Team</p>
                </div>
                <div class='footer'>
                    <p>Thank you for choosing us.</p>
                </div>
            </div>
        </body>
        </html>"
            };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_config.GetSection("SmtpSettings:Host").Value, 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config.GetSection("SmtpSettings:Username").Value, _config.GetSection("SmtpSettings:Password").Value);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }

        public async Task SendAccountResetPassword(string fullName, string userEmail, string OTP)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config.GetSection("SmtpSettings:Username").Value));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = "[ConstructionEquipmentRental Application] - Password Reset Request";
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
                    font-family: Arial, sans-serif;
                    background-color: #f9f9f9;
                    color: #333;
                    margin: 0;
                    padding: 0;
                }}
                .container {{
                    max-width: 700px;
                    margin: 0 auto;
                    padding: 20px;
                    background-color: #ffffff;
                    border-radius: 10px;
                    box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
                }}
                .header {{
                    background-color: #4caf50; /* Màu xanh nhẹ */
                    color: #fff;
                    padding: 20px;
                    text-align: center;
                    border-radius: 8px 8px 0 0;
                    box-shadow: 0 4px 10px rgba(0, 0, 0, 0.2);
                }}
                .header h1 {{
                    font-size: 36px;
                    font-weight: bold;
                    margin: 0;
                }}
                .body {{
                    padding: 20px;
                    background-color: #ffffff;
                    border-radius: 0 0 8px 8px;
                    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
                }}
                .body p {{
                    font-size: 16px;
                    line-height: 1.5;
                    color: #333;
                }}
                .otp {{
                    font-size: 24px;
                    font-weight: bold;
                    color: #4caf50; /* Màu xanh */
                }}
                .footer {{
                    padding: 10px;
                    text-align: center;
                    font-size: 14px;
                    color: #888;
                    background-color: #e0e0e0; /* Màu xám nhẹ */
                    border-top: 2px solid #bbb;
                    border-radius: 0 0 8px 8px;
                    box-shadow: 0 2px 5px rgba(0, 0, 0, 0.1);
                }}
            </style>
        </head>
        <body>
            <div class='container'>
                <div class='header'>
                    <h1>Password Reset Request</h1>
                </div>
                <div class='body'>
                    <p>Hi {fullName},</p>
                    <p>You have requested to reset your password. Please use the following OTP:</p>
                    <p class='otp'>{OTP}</p>
                    <p>This OTP is valid for a limited time. Please use it as soon as possible.</p>
                    <p>If you did not request a password reset, please ignore this email.</p>
                    <p>Thank you,</p>
                    <p>The ConstructionEquipmentRental Team</p>
                </div>
                <div class='footer'>
                    <p>Thank you for choosing us.</p>
                </div>
            </div>
        </body>
        </html>"
            };

            using var smtp = new MailKit.Net.Smtp.SmtpClient();
            await smtp.ConnectAsync(_config.GetSection("SmtpSettings:Host").Value, 587, MailKit.Security.SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(_config.GetSection("SmtpSettings:Username").Value, _config.GetSection("SmtpSettings:Password").Value);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }


        // Email tạo store
        public async Task SendStoreCreationEmail(string fullName, string userEmail, string storeName)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = "[SparkTech Ventures] - Xác nhận tạo cửa hàng của bạn";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Chào mừng bạn đến với SparkTech Ventures</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            margin: 0;
            padding: 0;
            background-color: #f9f9f9;
        }}
        .container {{
            max-width: 700px;
            margin: 0 auto;
            padding: 20px;
            background-color: #ffffff;
            border-radius: 10px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            text-align: center;
            padding: 20px;
            background-color: #ffc107;
            border-radius: 10px 10px 0 0;
        }}
        .header img {{
            max-width: 180px;
            margin-bottom: 10px;
        }}
        .header h1 {{
            font-size: 24px;
            font-weight: bold;
            color: #333;
            margin: 0;
        }}
        .body {{
            padding: 20px;
            background-color: #ffffff;
            border-radius: 0 0 10px 10px;
        }}
        .body p {{
            font-size: 16px;
            line-height: 1.5;
            color: #333;
        }}
        .footer {{
            padding: 10px;
            text-align: center;
            font-size: 14px;
            color: #888;
            border-top: 1px solid #ddd;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <img src='https://firebasestorage.googleapis.com/v0/b/marinepath-56521.appspot.com/o/Logo_SparkTech_Ventures.jpg?alt=media&token=c6654833-9929-4126-8501-c9383a751335' alt='SparkTech Ventures Logo'>
            <h1>Chúc mừng bạn đã tạo cửa hàng!</h1>
        </div>
        <div class='body'>
            <p>Xin chào {fullName},</p>
            <p>Cảm ơn bạn đã tạo cửa hàng <b>{storeName}</b> trên nền tảng <b>SparkTech Ventures</b>. Chúng tôi rất vui khi thấy bạn tham gia cộng đồng của chúng tôi!</p>
            <p>Hiện tại, chúng tôi đang tiến hành xác minh thông tin cửa hàng của bạn. Quá trình này có thể mất <b>tối đa 24 giờ</b>. Bạn sẽ nhận được thông báo khi cửa hàng của bạn được duyệt.</p>
            <p>Nếu bạn có bất kỳ câu hỏi nào, vui lòng liên hệ với chúng tôi qua email hoặc hotline hỗ trợ.</p>
        </div>
        <div class='footer'>
            <p>Trân trọng,</p>
            <p>Đội ngũ SparkTech Ventures</p>
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

        public async Task SendApprovalEmail(string fullName, string userEmail, string storeName, string adminName)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = "[SparkTech Ventures] - Cửa hàng của bạn đã được phê duyệt!";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Phê duyệt cửa hàng</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f9f9f9;
        }}
        .container {{
            max-width: 700px;
            margin: 20px auto;
            padding: 20px;
            background: #ffffff;
            border-radius: 10px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            background-color: #28a745;
            color: white;
            text-align: center;
            padding: 15px;
            border-radius: 10px 10px 0 0;
        }}
        .body {{
            padding: 20px;
        }}
        .footer {{
            text-align: center;
            font-size: 14px;
            color: #666;
            border-top: 1px solid #ddd;
            padding: 10px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>🎉 Chúc mừng! Cửa hàng của bạn đã được phê duyệt 🎉</h2>
        </div>
        <div class='body'>
            <p>Xin chào <b>{fullName}</b>,</p>
            <p>Chúng tôi vui mừng thông báo rằng cửa hàng <b>{storeName}</b> của bạn đã được phê duyệt bởi Admin <b>{adminName}</b>.</p>
            <p>Bây giờ, bạn có thể bắt đầu đăng bán sản phẩm và phát triển cửa hàng của mình trên nền tảng <b>SparkTech Ventures</b>.</p>
            <p>Nếu có bất kỳ câu hỏi nào, đừng ngần ngại liên hệ với chúng tôi!</p>
        </div>
        <div class='footer'>
            <p>Trân trọng,</p>
            <p>Đội ngũ SparkTech Ventures</p>
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
        public async Task SendRejectionEmail(string fullName, string userEmail, string storeName, string adminName, string reason)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["SmtpSettings:Username"]));
            email.To.Add(MailboxAddress.Parse(userEmail));
            email.Subject = "[SparkTech Ventures] - Cửa hàng của bạn không được duyệt";

            email.Body = new TextPart(MimeKit.Text.TextFormat.Html)
            {
                Text = $@"
<!DOCTYPE html>
<html lang='vi'>
<head>
    <meta charset='UTF-8'>
    <meta name='viewport' content='width=device-width, initial-scale=1.0'>
    <title>Từ chối cửa hàng</title>
    <style>
        body {{
            font-family: Arial, sans-serif;
            background-color: #f9f9f9;
        }}
        .container {{
            max-width: 700px;
            margin: 20px auto;
            padding: 20px;
            background: #ffffff;
            border-radius: 10px;
            box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
        }}
        .header {{
            background-color: #dc3545;
            color: white;
            text-align: center;
            padding: 15px;
            border-radius: 10px 10px 0 0;
        }}
        .body {{
            padding: 20px;
        }}
        .footer {{
            text-align: center;
            font-size: 14px;
            color: #666;
            border-top: 1px solid #ddd;
            padding: 10px;
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            <h2>⚠️ Rất tiếc! Cửa hàng của bạn không được phê duyệt ⚠️</h2>
        </div>
        <div class='body'>
            <p>Xin chào <b>{fullName}</b>,</p>
            <p>Chúng tôi rất tiếc khi phải thông báo rằng cửa hàng <b>{storeName}</b> của bạn đã bị từ chối bởi Admin <b>{adminName}</b>.</p>
            <p>Lý do: <b>{reason}</b></p>
            <p>Bạn có thể chỉnh sửa thông tin cửa hàng của mình và gửi lại yêu cầu duyệt.</p>
            <p>Nếu bạn cần hỗ trợ thêm, vui lòng liên hệ với chúng tôi.</p>
        </div>
        <div class='footer'>
            <p>Trân trọng,</p>
            <p>Đội ngũ SparkTech Ventures</p>
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
