using InvGen.Web.Models;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace InvGen.Web.Services
{
    public class PdfService : IPdfService
    {
        private readonly IQuotationService _quotationService;
        private readonly IConfiguration _configuration;

        public PdfService(IQuotationService quotationService, IConfiguration configuration)
        {
            _quotationService = quotationService;
            _configuration = configuration;
        }

        public async Task<byte[]> GenerateQuotationPdfAsync(int quotationId)
        {
            var quotation = await _quotationService.GetQuotationByIdAsync(quotationId);
            if (quotation == null)
                throw new ArgumentException("Quotation not found", nameof(quotationId));

            return await GenerateQuotationPdfAsync(quotation);
        }

        public Task<byte[]> GenerateQuotationPdfAsync(Quotation quotation)
        {
            using var memoryStream = new MemoryStream();
            var document = new Document(PageSize.A4, 40, 40, 40, 40);
            var writer = PdfWriter.GetInstance(document, memoryStream);

            document.Open();

            // Company information
            var companyName = _configuration["CompanySettings:Name"] ?? "Your Company Name";
            var companyAddress = _configuration["CompanySettings:Address"] ?? "Your Company Address";
            var companyPhone = _configuration["CompanySettings:Phone"] ?? "Your Phone Number";
            var companyEmail = _configuration["CompanySettings:Email"] ?? "your.email@company.com";

            // Fonts
            var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18, new BaseColor(64, 64, 64));
            var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12, new BaseColor(0, 0, 0));
            var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 10, new BaseColor(0, 0, 0));

            // Header
            var headerTable = new PdfPTable(2) { WidthPercentage = 100 };
            headerTable.SetWidths(new float[] { 60, 40 });

            // Company info cell
            var companyCell = new PdfPCell();
            companyCell.Border = Rectangle.NO_BORDER;
            companyCell.AddElement(new Paragraph(companyName, titleFont));
            companyCell.AddElement(new Paragraph(companyAddress, normalFont));
            companyCell.AddElement(new Paragraph($"Phone: {companyPhone}", normalFont));
            companyCell.AddElement(new Paragraph($"Email: {companyEmail}", normalFont));
            headerTable.AddCell(companyCell);

            // Quotation info cell
            var quotationCell = new PdfPCell();
            quotationCell.Border = Rectangle.NO_BORDER;
            quotationCell.HorizontalAlignment = Element.ALIGN_RIGHT;
            quotationCell.AddElement(new Paragraph("QUOTATION", titleFont));
            quotationCell.AddElement(new Paragraph($"Number: {quotation.QuotationNumber}", normalFont));
            quotationCell.AddElement(new Paragraph($"Date: {quotation.QuotationDate:dd/MM/yyyy}", normalFont));
            if (quotation.ValidUntil.HasValue)
                quotationCell.AddElement(new Paragraph($"Valid Until: {quotation.ValidUntil:dd/MM/yyyy}", normalFont));
            headerTable.AddCell(quotationCell);

            document.Add(headerTable);
            document.Add(new Paragraph(" ")); // Space

            // Customer information
            var customerTable = new PdfPTable(2) { WidthPercentage = 100 };
            customerTable.SetWidths(new float[] { 50, 50 });

            var billToCell = new PdfPCell();
            billToCell.Border = Rectangle.NO_BORDER;
            billToCell.AddElement(new Paragraph("Bill To:", headerFont));
            billToCell.AddElement(new Paragraph(quotation.Customer.Name, normalFont));
            if (!string.IsNullOrEmpty(quotation.Customer.CompanyName))
                billToCell.AddElement(new Paragraph(quotation.Customer.CompanyName, normalFont));
            if (!string.IsNullOrEmpty(quotation.Customer.Address))
                billToCell.AddElement(new Paragraph(quotation.Customer.Address, normalFont));
            if (!string.IsNullOrEmpty(quotation.Customer.City))
                billToCell.AddElement(new Paragraph($"{quotation.Customer.City}, {quotation.Customer.State} {quotation.Customer.PostalCode}", normalFont));
            billToCell.AddElement(new Paragraph($"Email: {quotation.Customer.Email}", normalFont));
            if (!string.IsNullOrEmpty(quotation.Customer.Phone))
                billToCell.AddElement(new Paragraph($"Phone: {quotation.Customer.Phone}", normalFont));
            customerTable.AddCell(billToCell);

            var projectCell = new PdfPCell();
            projectCell.Border = Rectangle.NO_BORDER;
            if (!string.IsNullOrEmpty(quotation.ProjectName))
            {
                projectCell.AddElement(new Paragraph("Project:", headerFont));
                projectCell.AddElement(new Paragraph(quotation.ProjectName, normalFont));
            }
            if (!string.IsNullOrEmpty(quotation.Description))
            {
                projectCell.AddElement(new Paragraph("Description:", headerFont));
                projectCell.AddElement(new Paragraph(quotation.Description, normalFont));
            }
            customerTable.AddCell(projectCell);

            document.Add(customerTable);
            document.Add(new Paragraph(" ")); // Space

            // Line items table
            var itemsTable = new PdfPTable(6) { WidthPercentage = 100 };
            itemsTable.SetWidths(new float[] { 8, 35, 12, 15, 15, 15 });

            // Table headers
            var headers = new[] { "S.No", "Description", "Qty", "Unit", "Rate", "Amount" };
            foreach (var header in headers)
            {
                var headerCell = new PdfPCell(new Phrase(header, headerFont))
                {
                    BackgroundColor = new BaseColor(211, 211, 211),
                    HorizontalAlignment = Element.ALIGN_CENTER,
                    Padding = 8
                };
                itemsTable.AddCell(headerCell);
            }

            // Line items
            int serialNo = 1;
            foreach (var item in quotation.LineItems.OrderBy(li => li.SortOrder))
            {
                itemsTable.AddCell(new PdfPCell(new Phrase(serialNo.ToString(), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });
                itemsTable.AddCell(new PdfPCell(new Phrase(item.Description, normalFont)) { Padding = 5 });
                itemsTable.AddCell(new PdfPCell(new Phrase(item.Quantity.ToString("0.##"), normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });
                itemsTable.AddCell(new PdfPCell(new Phrase(item.Unit, normalFont)) { HorizontalAlignment = Element.ALIGN_CENTER, Padding = 5 });
                itemsTable.AddCell(new PdfPCell(new Phrase($"₹{item.UnitPrice:N2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                itemsTable.AddCell(new PdfPCell(new Phrase($"₹{item.LineTotal:N2}", normalFont)) { HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
                serialNo++;
            }

            document.Add(itemsTable);
            document.Add(new Paragraph(" ")); // Space

            // Totals
            var totalsTable = new PdfPTable(2) { WidthPercentage = 100 };
            totalsTable.SetWidths(new float[] { 70, 30 });

            var emptyCell = new PdfPCell(new Phrase(" ")) { Border = Rectangle.NO_BORDER };
            totalsTable.AddCell(emptyCell);

            var totalsCell = new PdfPCell();
            totalsCell.Border = Rectangle.NO_BORDER;

            var subTotalTable = new PdfPTable(2) { WidthPercentage = 100 };
            subTotalTable.AddCell(new PdfPCell(new Phrase("Subtotal:", normalFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
            subTotalTable.AddCell(new PdfPCell(new Phrase($"₹{quotation.SubTotal:N2}", normalFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

            subTotalTable.AddCell(new PdfPCell(new Phrase($"GST ({quotation.TaxRate}%):", normalFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });
            subTotalTable.AddCell(new PdfPCell(new Phrase($"₹{quotation.TaxAmount:N2}", normalFont)) { Border = Rectangle.NO_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT });

            subTotalTable.AddCell(new PdfPCell(new Phrase("Total Amount:", headerFont)) { Border = Rectangle.TOP_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });
            subTotalTable.AddCell(new PdfPCell(new Phrase($"₹{quotation.TotalAmount:N2}", headerFont)) { Border = Rectangle.TOP_BORDER, HorizontalAlignment = Element.ALIGN_RIGHT, Padding = 5 });

            totalsCell.AddElement(subTotalTable);
            totalsTable.AddCell(totalsCell);

            document.Add(totalsTable);

            // Terms and conditions
            if (!string.IsNullOrEmpty(quotation.TermsAndConditions))
            {
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph("Terms and Conditions:", headerFont));
                document.Add(new Paragraph(quotation.TermsAndConditions, normalFont));
            }

            // Notes
            if (!string.IsNullOrEmpty(quotation.Notes))
            {
                document.Add(new Paragraph(" "));
                document.Add(new Paragraph("Notes:", headerFont));
                document.Add(new Paragraph(quotation.Notes, normalFont));
            }

            document.Add(new Paragraph(" "));
            document.Add(new Paragraph("Thank you for your business!", normalFont));

            document.Close();
            return Task.FromResult(memoryStream.ToArray());
        }
    }
}
