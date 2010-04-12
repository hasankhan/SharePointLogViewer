using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mail;

namespace SharePointLogViewer.Notifiers
{
    class EmailNotifier : INotifier
    {
        string sender;
        string recepients;
        string smtpServer;

        public EmailNotifier(string sender, string recepients, string smtpServer)
        {
            this.sender = sender;
            this.recepients = recepients;
            this.smtpServer = smtpServer;
        }

        #region INotifier Members

        public void Notify(LogEntryViewModel logEntry)
        {
            MailMessage message = new MailMessage();
            message.From = sender;
            message.To = recepients;
            message.Subject = "SharePoint Log";
            message.Body = logEntry.Message;
            SmtpMail.SmtpServer = smtpServer;
            SmtpMail.Send(message);
        }

        #endregion
    }
}
