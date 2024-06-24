using System;
using System.Collections.Generic;

namespace PdfInvoiceProcessor.Core.Models
{
    public class InvoiceData
    {
        public string? SupplierName { get; set; }
        public string? CustomerName { get; set; }
        public string? SupplierId { get; set; }
        public string? CustomerId { get; set; }
        public DateTime InvoiceDate { get; set; }
        public decimal TotalBeforeTax { get; set; }
        public decimal TotalWithTax { get; set; }
        public List<Product>? Products { get; set; }

        public override string ToString()
        {
            var productsInfo = string.Join(", ", Products?.ConvertAll(p => p.ToString()) ?? new List<string>());
            return $"SupplierName: {SupplierName}, CustomerName: {CustomerName}, SupplierId: {SupplierId}, CustomerId: {CustomerId}, InvoiceDate: {InvoiceDate}, TotalBeforeTax: {TotalBeforeTax}, TotalWithTax: {TotalWithTax}, Products: [{productsInfo}]";
        }
    }

    public class Product
    {
        public string? Name { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }

        public override string ToString()
        {
            return $"Name: {Name}, Quantity: {Quantity}, Price: {Price}";
        }
    }
}
