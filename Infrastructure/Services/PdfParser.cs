using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;
using Microsoft.Extensions.Logging;
using PdfInvoiceProcessor.Core.Interfaces;
using PdfInvoiceProcessor.Core.Models;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace PdfInvoiceProcessor.Infrastructure.Services
{
    public class PdfParser : IPdfParser
    {
        private readonly ILogger<PdfParser> _logger;

        public PdfParser(ILogger<PdfParser> logger)
        {
            _logger = logger;
        }

        public InvoiceData ExtractInvoiceData(string filePath)
        {
            _logger.LogInformation($"Starting extraction for file: {filePath}");
            var pdfReader = new PdfReader(filePath);
            var pdfDocument = new PdfDocument(pdfReader);
            var extractedData = new InvoiceData { Products = new List<Product>() };

            try
            {
                string fullText = "";

                for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
                {
                    var page = pdfDocument.GetPage(i);
                    var text = PdfTextExtractor.GetTextFromPage(page);
                    _logger.LogInformation($"Extracted text from page {i}: {text}");
                    fullText += text + "\n";
                }

                // Parse the full text to extract invoice details
                extractedData.SupplierName = Regex.Match(fullText, @"Supplier Information:\s*Name: (.*)").Groups[1].Value;
                extractedData.CustomerName = Regex.Match(fullText, @"Customer Information:\s*Name: (.*)").Groups[1].Value;
                extractedData.SupplierId = Regex.Match(fullText, @"Supplier ID: (\d+)").Groups[1].Value;
                extractedData.CustomerId = Regex.Match(fullText, @"Customer ID: (\d+)").Groups[1].Value;
                extractedData.InvoiceDate = DateTime.Parse(Regex.Match(fullText, @"Invoice Date: (\d{4}-\d{2}-\d{2})").Groups[1].Value);
                extractedData.TotalBeforeTax = decimal.Parse(Regex.Match(fullText, @"Subtotal: \$(\d+\.\d{2})").Groups[1].Value);
                extractedData.TotalWithTax = decimal.Parse(Regex.Match(fullText, @"Total: \$(\d+\.\d{2})").Groups[1].Value);

                var productMatches = Regex.Matches(fullText, @"Product Name: (.*)\s*Quantity: (\d+)\s*Unit Price: \$(\d+\.\d{2})\s*Total Price: \$(\d+\.\d{2})");
                foreach (Match match in productMatches)
                {
                    extractedData.Products.Add(new Product
                    {
                        Name = match.Groups[1].Value,
                        Quantity = int.Parse(match.Groups[2].Value),
                        Price = decimal.Parse(match.Groups[3].Value)
                    });
                }

                _logger.LogInformation($"Extracted Data: {extractedData}");
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error extracting data from PDF: {ex.Message}");
            }
            finally
            {
                pdfDocument.Close();
            }

            return extractedData;
        }
    }
}
