// using Google.Apis.Auth.OAuth2;
// using Google.Apis.Auth.OAuth2.Flows;
// using Google.Apis.Services;
// using Google.Apis.Vision.v1;
// using Google.Apis.Vision.v1.Data;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Logging;
// using PdfInvoiceProcessor.Core.Interfaces;
// using PdfInvoiceProcessor.Core.Models;
// using System;
// using System.Collections.Generic;
// using System.Globalization;
// using System.IO;
// using System.Text.RegularExpressions;

// namespace PdfInvoiceProcessor.Infrastructure.Services
// {
//     public class PdfParser : IPdfParser
//     {
//         private readonly ILogger<PdfParser> _logger;
//         private readonly IConfiguration _configuration;

//         public PdfParser(ILogger<PdfParser> logger, IConfiguration configuration)
//         {
//             _logger = logger;
//             _configuration = configuration;
//         }

//         public InvoiceData ExtractInvoiceData(string filePath)
//         {
//             _logger.LogInformation($"Starting extraction for file: {filePath}");
//             var extractedData = new InvoiceData { Products = new List<PdfInvoiceProcessor.Core.Models.Product>() };

//             try
//             {
//                 string fullText = ExtractTextWithCloudVision(filePath);

//                 if (string.IsNullOrEmpty(fullText))
//                 {
//                     _logger.LogWarning("Extracted text is null or empty");
//                     return extractedData;
//                 }

//                 _logger.LogInformation($"Full extracted text: {fullText}");

//                 extractedData.SupplierName = ExtractValue(fullText, @"שם הספק:\s*(.*?)\s");
//                 extractedData.CustomerName = ExtractValue(fullText, @"שם הלקוח:\s*(.*?)\s");
//                 extractedData.SupplierId = ExtractValue(fullText, @"ח\.פ ספק:\s*(\d+)");
//                 extractedData.CustomerId = ExtractValue(fullText, @"ח\.פ לקוח:\s*(\d+)");
//                 extractedData.InvoiceDate = DateTime.ParseExact(ExtractValue(fullText, @"תאריך חשבונית:\s*(\d{2}/\d{2}/\d{4})"), "dd/MM/yyyy", CultureInfo.InvariantCulture);
//                 extractedData.TotalBeforeTax = decimal.Parse(ExtractValue(fullText, @"סך הכל לפני מע""מ:\s*₪(\d+\.\d{2})"));
//                 extractedData.TotalWithTax = decimal.Parse(ExtractValue(fullText, @"סך הכל כולל מע""מ:\s*₪(\d+\.\d{2})"));

//                 var productMatches = Regex.Matches(fullText, @"שם המוצר:\s*(.*?)\s*כמות:\s*(\d+)\s*מחיר ליחידה:\s*₪(\d+\.\d{2})\s*סה""כ:\s*₪(\d+\.\d{2})");
//                 foreach (Match match in productMatches)
//                 {
//                     extractedData.Products.Add(new PdfInvoiceProcessor.Core.Models.Product
//                     {
//                         Name = match.Groups[1].Value.Trim(),
//                         Quantity = int.Parse(match.Groups[2].Value.Trim()),
//                         Price = decimal.Parse(match.Groups[3].Value.Trim())
//                     });
//                 }

//                 _logger.LogInformation($"Extracted Data: {extractedData}");
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error extracting data from PDF: {ex.Message}");
//             }

//             return extractedData;
//         }

//         private string ExtractTextWithCloudVision(string filePath)
//         {
//             try
//             {
//                 var accessToken = _configuration["Vision:AccessToken"];
//                 var refreshToken = _configuration["Vision:RefreshToken"];
//                 var clientId = _configuration["Vision:ClientId"];
//                 var clientSecret = _configuration["Vision:ClientSecret"];

//                 var tokenResponse = new Google.Apis.Auth.OAuth2.Responses.TokenResponse
//                 {
//                     AccessToken = accessToken,
//                     RefreshToken = refreshToken,
//                     ExpiresInSeconds = 3599,
//                     IssuedUtc = DateTime.UtcNow
//                 };

//                 var clientSecrets = new Google.Apis.Auth.OAuth2.ClientSecrets
//                 {
//                     ClientId = clientId,
//                     ClientSecret = clientSecret
//                 };

//                 var credential = new UserCredential(new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
//                 {
//                     ClientSecrets = clientSecrets
//                 }), "user", tokenResponse);

//                 if (credential.Token.IsExpired(credential.Flow.Clock))
//                 {
//                     if (!credential.RefreshTokenAsync(System.Threading.CancellationToken.None).Result)
//                     {
//                         throw new InvalidOperationException("Failed to refresh the access token.");
//                     }
//                 }

//                 var visionService = new VisionService(new BaseClientService.Initializer
//                 {
//                     HttpClientInitializer = credential,
//                     ApplicationName = "PdfInvoiceProcessor"
//                 });

//                 var pdfBytes = File.ReadAllBytes(filePath);
//                 if (pdfBytes == null || pdfBytes.Length == 0)
//                 {
//                     _logger.LogError("PDF file is empty or could not be read");
//                     return null;
//                 }

//                 var image = new Google.Apis.Vision.v1.Data.Image
//                 {
//                     Content = Convert.ToBase64String(pdfBytes)
//                 };

//                 var request = new AnnotateImageRequest
//                 {
//                     Image = image,
//                     Features = new List<Feature> { new Feature { Type = "DOCUMENT_TEXT_DETECTION" } }
//                 };

//                 var batchRequest = new BatchAnnotateImagesRequest { Requests = new List<AnnotateImageRequest> { request } };
//                 var batchResponse = visionService.Images.Annotate(batchRequest).Execute();

//                 if (batchResponse.Responses == null || batchResponse.Responses.Count == 0)
//                 {
//                     _logger.LogError("Vision API response is null or empty");
//                     return null;
//                 }

//                 var response = batchResponse.Responses[0];
//                 if (response.Error != null)
//                 {
//                     _logger.LogError($"Vision API error: {response.Error.Message}");
//                     return null;
//                 }

//                 var fullTextAnnotation = response?.FullTextAnnotation?.Text;
//                 if (string.IsNullOrEmpty(fullTextAnnotation))
//                 {
//                     _logger.LogWarning("FullTextAnnotation is null or empty");
//                 }

//                 return fullTextAnnotation;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error during Cloud Vision API call: {ex.Message}");
//                 return null;
//             }
//         }

//         private string ExtractValue(string text, string pattern)
//         {
//             var match = Regex.Match(text, pattern);
//             if (match.Success)
//             {
//                 _logger.LogInformation($"Pattern matched for {pattern}, value: {match.Groups[1].Value.Trim()}");
//                 return match.Groups[1].Value.Trim();
//             }
//             else
//             {
//                 _logger.LogWarning($"Pattern not matched: {pattern}");
//                 return string.Empty;
//             }
//         }
//     }
// }
