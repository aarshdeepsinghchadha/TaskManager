using Application.Common;
using Application.Interface;
using log4net;
using log4net.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using RestSharp;
using SendGrid;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Infrastructure.Services
{
    public class EmailSenderService : IEmailSenderService
    {
        private readonly IConfiguration _configuration;
        private readonly IResponseGeneratorService _responseGeneratorService;
        private readonly ILog _log;
      
        public EmailSenderService(IConfiguration configuration, IResponseGeneratorService responseGeneratorService)
        {
            _configuration = configuration;
            _responseGeneratorService = responseGeneratorService;
            _log = LogManager.GetLogger(typeof(EmailSenderService));
          
        }

        public async Task<ReturnResponse> SendEmailByMailgunAsync(string toEmail, string subject, string body)
        {
            try
            {
                var apiKey = _configuration["MailgunSettings:ApiKey"];
                var domain = _configuration["MailgunSettings:Domain"];

                var client = new RestClient($"https://api.mailgun.net/v3/{domain}");
                var request = new RestRequest("messages", Method.Post);
                var base64ApiKey = Convert.ToBase64String(Encoding.ASCII.GetBytes($"api:{apiKey}"));
                request.AddHeader("Authorization", $"Basic {base64ApiKey}");
                request.AddParameter("from", "bhavesh.c@indianic.com");
                request.AddParameter("to", toEmail);
                request.AddParameter("subject", subject);
                request.AddParameter("text", body);
                request.AddParameter("html", body);

                var response = await client.ExecuteAsync(request);

                if (response.IsSuccessful)
                {
                    _log.Info($"Email sent successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(true, StatusCodes.Status200OK, "Email sent successfully ");
                }
                else
                {
                    _log.Error($"Failed to send email. {response.ErrorMessage}");
                    var errorMessage = $"Failed to send email. {response.ErrorMessage}";
                    return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status500InternalServerError, errorMessage);
                }
            }
            catch (Exception ex)
            {
                _log.Error($"Error ocurred while sending mail {ex.Message}");
                var errorMessage = $"Error ocurred while sending mail {ex.Message}";
                return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status500InternalServerError, errorMessage);
            }
        }

        /// <summary>
        /// Send Email using SendGrid 
        /// </summary>
        /// <param name="toEmail"></param>
        /// <param name="subject"></param>
        /// <param name="body"></param>
        /// <returns></returns>
        public async Task<ReturnResponse> SendEmailBySendGridAsync(string toEmail, string subject, string body)
        {
            try
            {

                var apiKey = _configuration["SendGrid:ApiKey"];
                var client = new SendGridClient(apiKey);

                var from = new EmailAddress(_configuration["SendGrid:SenderEmail"], _configuration["SendGrid:SenderName"]);
                var to = new EmailAddress(toEmail);

                var plainTextContent = body;
                var htmlContent = body;
                var messsage = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

                var response = await client.SendEmailAsync(messsage);

                if (response.IsSuccessStatusCode)
                {
                    // Email sent successfully
                    _log.Info("Email sent successfully");
                    return await _responseGeneratorService.GenerateResponseAsync(
                        true, StatusCodes.Status200OK, "Email sent successfully.");
                }
                else
                {
                    // Email sending failed
                    _log.Error("Failed to send email using sendgrid");
                    return await _responseGeneratorService.GenerateResponseAsync(
                        false, StatusCodes.Status500InternalServerError, "Failed to send email StatusCode: " + response.StatusCode);
                }

            }
            catch (Exception ex)
            {
                _log.Error($"Error ocurred while sending mail {ex.Message}");
                return await _responseGeneratorService.GenerateResponseAsync(false, StatusCodes.Status500InternalServerError, ex.Message);
            }

        }



    }
}
