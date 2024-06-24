using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PdfInvoiceProcessor.Core.Interfaces;
using PdfInvoiceProcessor.Core.Models;
using System.IO;
using System.Threading.Tasks;

namespace PdfInvoiceProcessor.API.Controllers
{
    [ApiController]
    [Route("api/pdf")]
    public class PdfController : ControllerBase
    {
        private readonly IPdfParser _pdfParser;
        private readonly IEmailService _emailService;
        private readonly ILogger<PdfController> _logger;

        public PdfController(IPdfParser pdfParser, IEmailService emailService, ILogger<PdfController> logger)
        {
            _pdfParser = pdfParser;
            _emailService = emailService;
            _logger = logger;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadPdf([FromForm] IFormFile pdfFile, [FromForm] string email)
        {
            _logger.LogInformation("Received file upload request");

            if (pdfFile == null || pdfFile.Length == 0)
            {
                _logger.LogWarning("No file uploaded or file is empty");
                return BadRequest("No file uploaded.");
            }

            if (string.IsNullOrEmpty(email))
            {
                _logger.LogWarning("No email provided");
                return BadRequest("No email provided.");
            }

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "uploads");

            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            var filePath = Path.Combine(uploadsDir, pdfFile.FileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await pdfFile.CopyToAsync(stream);
                _logger.LogInformation($"File saved to {filePath}");
            }

            _logger.LogInformation("Extracting data from PDF");
            var extractedData = _pdfParser.ExtractInvoiceData(filePath);

            if (extractedData == null)
            {
                _logger.LogWarning("No data extracted from PDF");
                return Ok(new { Message = "File uploaded, but no data extracted.", Data = extractedData });
            }

            // Log the extracted data
            _logger.LogInformation($"Extracted Data: {extractedData}");

            // Send the email with the extracted data
            _logger.LogInformation("Sending email with invoice data");
            await _emailService.SendEmailWithInvoiceData(filePath, extractedData, email);
            _logger.LogInformation("Email sent successfully");

            return Ok(new { Message = "File uploaded, processed successfully, and email sent.", Data = extractedData });
        }
    }
}
