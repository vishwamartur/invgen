using InvGen.Web.Models;

namespace InvGen.Web.Services
{
    public interface IPdfService
    {
        Task<byte[]> GenerateQuotationPdfAsync(Quotation quotation);
        Task<byte[]> GenerateQuotationPdfAsync(int quotationId);
    }
}
