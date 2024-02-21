using Application.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IEmailSenderService
    {
        Task<ReturnResponse>  SendEmailByMailgunAsync(string toEmail, string subject, string body);
        Task<ReturnResponse> SendEmailBySendGridAsync(string toEmail, string subject, string body);
    }
}
