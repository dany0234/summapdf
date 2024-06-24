// using Google.Apis.Auth.OAuth2;
// using Google.Apis.Auth.OAuth2.Flows;
// using Google.Apis.Auth.OAuth2.Responses;
// using MimeKit;
// using MailKit.Net.Smtp;
// using PdfInvoiceProcessor.Core.Interfaces;
// using PdfInvoiceProcessor.Core.Models;
// using Microsoft.Extensions.Configuration;
// using System.Threading.Tasks;
// using System;
// using MailKit.Security;

// namespace PdfInvoiceProcessor.Infrastructure.Services
// {
//     public class EmailService : IEmailService
//     {
//         private readonly IConfiguration _configuration;

//         public EmailService(IConfiguration configuration)
//         {
//             _configuration = configuration;
//         }

//         public async Task SendEmailWithInvoiceData(string filePath, InvoiceData data)
//         {
//             var message = new MimeMessage();
//             message.From.Add(new MailboxAddress("החברה שלך", _configuration["Email:From"]));
//             message.To.Add(new MailboxAddress("נמען", _configuration["Email:To"]));
//             message.Subject = "פרטי החשבונית";

//             var bodyBuilder = new BodyBuilder();
//             bodyBuilder.TextBody = $"פרטי החשבונית:\n\nשם הספק: {data.SupplierName}\nשם הלקוח: {data.CustomerName}\nסך הכל לפני מע\"מ: {data.TotalBeforeTax}\nסך הכל כולל מע\"מ: {data.TotalWithTax}";
//             bodyBuilder.Attachments.Add(filePath);

//             message.Body = bodyBuilder.ToMessageBody();

//             var tokenResponse = new TokenResponse
//             {
//                 AccessToken = _configuration["Email:AccessToken"],
//                 RefreshToken = _configuration["Email:RefreshToken"],
//                 ExpiresInSeconds = 3599,
//                 IssuedUtc = DateTime.UtcNow
//             };

//             var clientSecrets = new ClientSecrets
//             {
//                 ClientId = _configuration["Email:ClientId"],
//                 ClientSecret = _configuration["Email:ClientSecret"]
//             };

//             var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
//             {
//                 ClientSecrets = clientSecrets
//             });

//             var credential = new UserCredential(flow, "user", tokenResponse);

//             if (credential.Token.IsExpired(credential.Flow.Clock))
//             {
//                 if (!await credential.RefreshTokenAsync(CancellationToken.None))
//                 {
//                     throw new InvalidOperationException("Failed to refresh the access token.");
//                 }
//             }

//             using (var client = new SmtpClient())
//             {
//                 await client.ConnectAsync("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
//                 var oauth2 = new SaslMechanismOAuth2(_configuration["Email:Username"], credential.Token.AccessToken);
//                 await client.AuthenticateAsync(oauth2);
//                 await client.SendAsync(message);
//                 await client.DisconnectAsync(true);
//             }
//         }
//     }
// }
