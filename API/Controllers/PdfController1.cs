// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Text;
// using Microsoft.Extensions.Configuration;
// using Microsoft.Extensions.Logging;
// using PdfInvoiceProcessor.Core.Interfaces;
// using PdfInvoiceProcessor.Core.Models;
// using Tesseract;
// using UglyToad.PdfPig;
// using UglyToad.PdfPig.Content;

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
//                 string fullText = ExtractTextFromPdfWithOcr(filePath);

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

//         private string ExtractTextFromPdfWithOcr(string filePath)
//         {
//             try
//             {
//                 // Extract images from the PDF using PdfPig
//                 List<byte[]> images = ExtractImagesFromPdf(filePath);

//                 // Perform OCR on each image
//                 var fullText = new StringBuilder();
//                 foreach (var image in images)
//                 {
//                     fullText.AppendLine(PerformOcrOnImage(image));
//                 }

//                 return fullText.ToString();
//             }
//             catch (Exception ex)
//             {
//                 _logger.LogError($"Error extracting text from PDF: {ex.Message}");
//                 return null;
//             }
//         }

//         private List<byte[]> ExtractImagesFromPdf(string filePath)
//         {
//             var images = new List<byte[]>();
//             using (var document = PdfDocument.Open(filePath))
//             {
//                 foreach (var page in document.GetPages())
//                 {
//                     foreach (var image in page.GetImages())
//                     {
//                         using (var memoryStream = new MemoryStream())
//                         {
//                             image.WriteToStream(memoryStream);
//                             images.Add(memoryStream.ToArray());
//                         }
//                     }
//                 }
//             }
//             return images;
//         }

//         private string PerformOcrOnImage(byte[] image)
//         {
//             var tessDataPath = _configuration["Tesseract:TessDataPath"];
//             var language = "heb"; // Hebrew language code

//             using (var engine = new TesseractEngine(tessDataPath, language, EngineMode.Default))
//             {
//                 using (var img = Pix.LoadFromMemory(image))
//                 {
//                     using (var page = engine.Process(img))
//                     {
//                         return page.GetText();
//                     }
//                 }
//             }
//         }
//     }
// }
