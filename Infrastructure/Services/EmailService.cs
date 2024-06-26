using SendGrid;
using SendGrid.Helpers.Mail;
using PdfInvoiceProcessor.Core.Interfaces;
using PdfInvoiceProcessor.Core.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;
using System.IO;

namespace PdfInvoiceProcessor.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailWithInvoiceData(string filePath, InvoiceData data, string recipientEmail)
        {
            var apiKey = _configuration["SendGrid:ApiKey"];
            var client = new SendGridClient(apiKey);
            var from = new EmailAddress(_configuration["Email:From"], "Summa Company");
            var to = new EmailAddress(recipientEmail);
            var subject = "Invoice Details";
            var plainTextContent = $"Invoice details:\n\n" +
                                   $"Supplier: {data.SupplierName}\n" +
                                   $"Customer: {data.CustomerName}\n" +
                                   $"Supplier ID: {data.SupplierId}\n" +
                                   $"Customer ID: {data.CustomerId}\n" +
                                   $"Invoice Date: {data.InvoiceDate:dd/MM/yyyy}\n" +
                                   $"Total Before Tax: {data.TotalBeforeTax}\n" +
                                   $"Total With Tax: {data.TotalWithTax}\n" +
                                   $"Products:\n";

            foreach (var product in data.Products)
            {
                plainTextContent += $"- Name: {product.Name}, Quantity: {product.Quantity}, Price: {product.Price}\n";
            }

            var htmlContent = plainTextContent.Replace("\n", "<br>");
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent, htmlContent);

            var attachment = new SendGrid.Helpers.Mail.Attachment
            {
                Content = Convert.ToBase64String(File.ReadAllBytes(filePath)),
                Filename = Path.GetFileName(filePath),
                Type = "application/pdf",
                Disposition = "attachment"
            };
            msg.AddAttachment(attachment);

            _logger.LogInformation("Sending email to {RecipientEmail}", recipientEmail);
            var response = await client.SendEmailAsync(msg);

            _logger.LogInformation("SendGrid response status code: {StatusCode}", response.StatusCode);
            _logger.LogInformation("SendGrid response body: {ResponseBody}", await response.Body.ReadAsStringAsync());
            if (response.Headers != null)
            {
                foreach (var header in response.Headers)
                {
                    _logger.LogInformation("SendGrid response header: {HeaderKey} - {HeaderValue}", header.Key, string.Join(",", header.Value));
                }
            }
        }
    }
}
