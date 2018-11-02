
using System.Net.Mail;

namespace Netpips.Core.Service
{
    public interface ISmtpService
    {
        void Send(MailMessage email);
    }
}