// using System;
// using System.IO;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Logging;
// using PdfInvoiceProcessor.Core.Interfaces;
// using PdfInvoiceProcessor.Core.Models;
// using Spire.Pdf;

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
//                 string fullText = ExtractTextFromPdf(filePath);

//                 if (string.IsNullOrEmpty(fullText))
//                 {
//                     _logger.LogWarning("Extracted text is null or empty");
//                     return extractedData;
//                 }

//                 _logger.LogInformation($"Full extracted text: {fullText}");

//                 // Process extracted text (omitted for brevity)
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error extracting data from PDF: {ex.Message}");
//             }

//             return extractedData;
//         }

//         private string ExtractTextFromPdf(string filePath)
//         {
//             try
//             {
//                 // Load the PDF document
//                 PdfDocument document = new PdfDocument();
//                 document.LoadFromFile(filePath);

//                 // Extract text from each page
//                 string fullText = "";
//                 foreach (PdfPageBase page in document.Pages)
//                 {
//                     fullText += page.ExtractText();
//                 }

//                 return fullText;
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error extracting text from PDF: {ex.Message}");
//                 return null;
//             }
//         }
//     }
// }
