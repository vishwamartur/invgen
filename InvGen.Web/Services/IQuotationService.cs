using InvGen.Web.Models;

namespace InvGen.Web.Services
{
    public interface IQuotationService
    {
        Task<IEnumerable<Quotation>> GetAllQuotationsAsync();
        Task<IEnumerable<Quotation>> GetQuotationsByCustomerAsync(int customerId);
        Task<IEnumerable<Quotation>> GetQuotationsByStatusAsync(QuotationStatus status);
        Task<Quotation?> GetQuotationByIdAsync(int id);
        Task<Quotation?> GetQuotationByNumberAsync(string quotationNumber);
        Task<Quotation> CreateQuotationAsync(Quotation quotation);
        Task<Quotation> UpdateQuotationAsync(Quotation quotation);
        Task<bool> DeleteQuotationAsync(int id);
        Task<IEnumerable<Quotation>> SearchQuotationsAsync(string searchTerm);
        Task<bool> QuotationExistsAsync(int id);
        Task<string> GenerateQuotationNumberAsync();
        
        // Line item methods
        Task<QuotationLineItem> AddLineItemAsync(QuotationLineItem lineItem);
        Task<QuotationLineItem> UpdateLineItemAsync(QuotationLineItem lineItem);
        Task<bool> DeleteLineItemAsync(int lineItemId);
        Task<bool> UpdateQuotationTotalsAsync(int quotationId);
        
        // Status management
        Task<bool> UpdateQuotationStatusAsync(int quotationId, QuotationStatus status);
        Task<IEnumerable<Quotation>> GetExpiredQuotationsAsync();
    }
}
