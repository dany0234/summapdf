using PdfInvoiceProcessor.Core.Models;

namespace PdfInvoiceProcessor.Core.Interfaces
{
    public interface IPdfParser
    {
        InvoiceData ExtractInvoiceData(string filePath);
    }
}
