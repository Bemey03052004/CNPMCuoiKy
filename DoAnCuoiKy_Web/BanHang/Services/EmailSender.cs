using MimeKit;
using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace BanHang.Services
{
	public class EmailSender
	{
		private readonly IConfiguration _config;

		public EmailSender(IConfiguration config)
		{
			_config = config;
		}

		public async Task SendEmailAsync(string toEmail, string subject, string message)
		{
			var email = new MimeMessage();
			email.From.Add(new MailboxAddress("Admin", _config["EmailSettings:SenderEmail"]));
			email.To.Add(new MailboxAddress("", toEmail));
			email.Subject = subject;

			email.Body = new TextPart("html") { Text = message };

			using var smtp = new SmtpClient();
			await smtp.ConnectAsync(_config["EmailSettings:SmtpServer"], int.Parse(_config["EmailSettings:Port"]), false);
			await smtp.AuthenticateAsync(_config["EmailSettings:SenderEmail"], _config["EmailSettings:SenderPassword"]);
			await smtp.SendAsync(email);
			await smtp.DisconnectAsync(true);
		}
	}
}