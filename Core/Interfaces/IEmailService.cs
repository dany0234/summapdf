using System.Threading.Tasks;
using PdfInvoiceProcessor.Core.Models;

namespace PdfInvoiceProcessor.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailWithInvoiceData(string filePath, InvoiceData data, string recipientEmail);
    }
}
