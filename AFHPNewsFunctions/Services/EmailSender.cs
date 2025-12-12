using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using MimeKit.Text;

namespace AFHPNewsFunctions.Services
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _configuration;

        public EmailSender(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<string?> SendEmailWithDelayAsync(string email, string subject, string htmlMessage, int minDelayMS, int maxDelayMS)
        {
            await Task.Delay(TimeSpan.FromMilliseconds(new Random().Next(minDelayMS, maxDelayMS)));

            return await SendEmailAsync(email, subject, htmlMessage);
        }

        public async Task<string?> SendEmailAsync(string email, string subject, string htmlMessage)
        {
            string? response = null;

            var sender = MailboxAddress.Parse(_configuration["Email.SenderEmail"]);
            sender.Name = _configuration["Email.SenderName"];

            var message = new MimeMessage() {
                Sender = sender,
                Subject = subject,
            };

            message.To.Add(MailboxAddress.Parse(email));
            message.From.Add(message.Sender);

            // We will say we are sending HTML. But there are options for plaintext etc.
            message.Body = new TextPart(TextFormat.Html) { Text = htmlMessage };

            using (var emailClient = new SmtpClient())
            {
                try
                {
                    // The last parameter here is to use SSL
                    await emailClient.ConnectAsync(_configuration["Email.SmtpServer"], Convert.ToInt32(_configuration["Email.SmtpPort"]), true);
                }
                catch (SmtpCommandException ex)
                {
                    response = "Error trying to connect:" + ex.Message + " StatusCode: " + ex.StatusCode;
                    return response;
                }
                catch (SmtpProtocolException ex)
                {
                    response = "Protocol error while trying to connect:" + ex.Message;
                    return response;
                }

                // Remove any OAuth functionality as we won't be using it now
                emailClient.AuthenticationMechanisms.Remove("XOAUTH2");

                await emailClient.AuthenticateAsync(_configuration["Email.SmtpUsername"], _configuration["Email.SmtpPassword"]);
                try
                {
                    await emailClient.SendAsync(message);
                }
                catch (SmtpCommandException ex)
                {
                    response = "Error sending message: " + ex.Message + " StatusCode: " + ex.StatusCode;
                    switch (ex.ErrorCode)
                    {
                        case SmtpErrorCode.RecipientNotAccepted:
                            response += " Recipient not accepted: " + ex.Mailbox;
                            break;
                        case SmtpErrorCode.SenderNotAccepted:
                            response += " Sender not accepted: " + ex.Mailbox;
                            Console.WriteLine("\tSender not accepted: {0}", ex.Mailbox);
                            break;
                        case SmtpErrorCode.MessageNotAccepted:
                            response += " Message not accepted.";
                            break;
                    }
                }

                await emailClient.DisconnectAsync(true);
            }

            return response;
        }
    }
}
