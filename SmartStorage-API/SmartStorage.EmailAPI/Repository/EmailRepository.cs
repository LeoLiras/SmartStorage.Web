using SmartStorage.EmailAPI.Repository.Interfaces;
using SmartStorage_Shared.VO;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text.RegularExpressions;

namespace SmartStorage.EmailAPI.Repository
{
    public class EmailRepository : IEmailRepository
    {
        #region Properties

        public string Provedor { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }

        #endregion

        #region Constructros

        public EmailRepository(string provedor, string username, string password)
        {
            Provedor = provedor;
            Username = username;
            Password = password;
        }

        #endregion

        #region Methods

        public async Task NewProductEmail(ProductVO product)
        {
            throw new NotImplementedException();
        }

        public void SendEmail(List<string> emailsTo, string subject, string body, List<string> attachments = null)
        {
            var message = PrepareteMessage(emailsTo, subject, body, attachments);

            SendEmailBySmtp(message);
        }

        private MailMessage PrepareteMessage(List<string> emailsTo, string subject, string body, List<string> attachments)
        {
            var mail = new MailMessage();
            mail.From = new MailAddress(Username);

            foreach (var email in emailsTo)
            {
                if (ValidateEmail(email))
                    mail.To.Add(email);
            }

            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            if (attachments != null)
            {
                foreach (var file in attachments)
                {
                    var data = new Attachment(file, MediaTypeNames.Application.Octet);
                    ContentDisposition disposition = data.ContentDisposition;
                    disposition.CreationDate = File.GetCreationTime(file);
                    disposition.ModificationDate = File.GetLastWriteTime(file);
                    disposition.ReadDate = File.GetLastAccessTime(file);

                    mail.Attachments.Add(data);
                }
            }

            return mail;
        }

        private bool ValidateEmail(string email)
        {
            Regex expression = new Regex(@"\w+@[a-zA-Z_]+?\.[a-zA-Z]{2,3}");
            if (expression.IsMatch(email))
                return true;

            return false;
        }

        private void SendEmailBySmtp(MailMessage message)
        {
            SmtpClient smtpClient = new SmtpClient("smtp.office365.com");
            smtpClient.Host = Provedor;
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.Timeout = 50000;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(Username, Password);
            smtpClient.Send(message);
            smtpClient.Dispose();
        }

        #endregion

    }
}
