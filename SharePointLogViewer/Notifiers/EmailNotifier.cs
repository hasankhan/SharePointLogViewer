using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace SharePointLogViewer.Notifiers
{
    class EmailNotifier : INotifier
    {
        string sender;
        string recepients;
        string smtpServer;
        SmtpClient client;

        public EmailNotifier(string sender, string recepients, string smtpServer)
        {
            this.sender = sender;
            this.recepients = recepients;
            this.smtpServer = smtpServer;
            client = new SmtpClient(smtpServer);
        }

        #region INotifier Members

        public void Notify(LogEntryViewModel logEntry)
        {
            MailMessage message = new MailMessage(sender, recepients);
            message.Subject = "SharePoint Log";
            message.Body = logEntry.Message;            
            client.Send(message);
        }

        #endregion

        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}
