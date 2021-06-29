using System;
using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Netpips.Core.Settings;

namespace Netpips.Core.Service
{
    public class GmailSmtpClient : ISmtpService
    {
        private readonly SmtpClient client;

        private readonly ILogger<GmailSmtpClient> logger;

        private readonly MailAddress netpipsAddress;

        public GmailSmtpClient(ILogger<GmailSmtpClient> logger, IOptions<GmailMailerAccountSettings> options)
        {
            this.logger = logger;
            netpipsAddress = new MailAddress(options.Value.Username, "Netpips");
            client = new SmtpClient
            {
                Port = 587,
                Host = "smtp.gmail.com",
                UseDefaultCredentials = false,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                Credentials = new NetworkCredential(netpipsAddress.Address, options.Value.Password),
            };
        }

        public void Send(MailMessage email)
        {
            email.From = netpipsAddress;
            try
            {
                logger.LogInformation("[Send] Sending");
                client.Send(email);
                logger.LogInformation("[Send] Success");
            }
            catch (Exception ex)
            {
                logger.LogWarning("[Send] Faileure");
                logger.LogWarning(ex.Message);
            }
        }
    }
}