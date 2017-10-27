using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Threading.Tasks;

namespace Host.Services
{
    // This class is used by the application to send email for account confirmation and password reset.
    // For more details see https://go.microsoft.com/fwlink/?LinkID=532713
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            try
            {
                var myMessage = new MailMessage();

                myMessage.To.Add(email);

                myMessage.From = new MailAddress("postmaster@blackstoneonline.com", "BlackStone Identity");

                myMessage.Subject = subject;

                myMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message, null, MediaTypeNames.Text.Plain));
                myMessage.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(message, null, MediaTypeNames.Text.Html));

                SmtpClient smtpClient = new SmtpClient("appmail.pinserve.local", Convert.ToInt32(25));

                NetworkCredential credentials = new NetworkCredential("postmaster", "bl@ckstone");

                smtpClient.Credentials = credentials;

                smtpClient.Send(myMessage);

                return Task.FromResult(0);
            }
            catch (Exception x)
            {

                throw x;
            }
        }
    }
}
