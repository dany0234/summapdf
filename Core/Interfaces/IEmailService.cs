using PdfInvoiceProcessor.Core.Models;
using System.Threading.Tasks;

namespace PdfInvoiceProcessor.Core.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailWithInvoiceData(string filePath, InvoiceData data);
    }
}
