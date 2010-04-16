using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Mail;

namespace SharePointLogViewer.Notifiers
{
    class EmailNotifier: INotifier
    {
        string sender;
        string recepients;
        string smtpServer;
        SmtpClient client;

        public EmailNotifier(string sender, string recepients, string smtpServer)
        {
            if (String.IsNullOrEmpty(sender))
                throw new ArgumentNullException("sender");
            if (String.IsNullOrEmpty(recepients))
                throw new ArgumentNullException("recepients");
            if (String.IsNullOrEmpty(smtpServer))
                throw new ArgumentNullException("smtpServer");

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
    }
}
