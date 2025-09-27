using InvGen.Web.Data;
using InvGen.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace InvGen.Web.Services
{
    public class QuotationService : IQuotationService
    {
        private readonly InvGenDbContext _context;
        private readonly ITaxCalculationService _taxCalculationService;

        public QuotationService(InvGenDbContext context, ITaxCalculationService taxCalculationService)
        {
            _context = context;
            _taxCalculationService = taxCalculationService;
        }

        public async Task<IEnumerable<Quotation>> GetAllQuotationsAsync()
        {
            return await _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.LineItems)
                    .ThenInclude(li => li.Product)
                .OrderByDescending(q => q.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quotation>> GetQuotationsByCustomerAsync(int customerId)
        {
            return await _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.LineItems)
                    .ThenInclude(li => li.Product)
                .Where(q => q.CustomerId == customerId)
                .OrderByDescending(q => q.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Quotation>> GetQuotationsByStatusAsync(QuotationStatus status)
        {
            return await _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.LineItems)
                    .ThenInclude(li => li.Product)
                .Where(q => q.Status == status)
                .OrderByDescending(q => q.CreatedDate)
                .ToListAsync();
        }

        public async Task<Quotation?> GetQuotationByIdAsync(int id)
        {
            return await _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.LineItems)
                    .ThenInclude(li => li.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(q => q.Id == id);
        }

        public async Task<Quotation?> GetQuotationByNumberAsync(string quotationNumber)
        {
            return await _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.LineItems)
                    .ThenInclude(li => li.Product)
                        .ThenInclude(p => p.Category)
                .FirstOrDefaultAsync(q => q.QuotationNumber == quotationNumber);
        }

        public async Task<Quotation> CreateQuotationAsync(Quotation quotation)
        {
            if (string.IsNullOrEmpty(quotation.QuotationNumber))
            {
                quotation.QuotationNumber = await GenerateQuotationNumberAsync();
            }

            quotation.CreatedDate = DateTime.UtcNow;
            _context.Quotations.Add(quotation);
            await _context.SaveChangesAsync();
            return quotation;
        }

        public async Task<Quotation> UpdateQuotationAsync(Quotation quotation)
        {
            quotation.UpdatedDate = DateTime.UtcNow;
            _context.Entry(quotation).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return quotation;
        }

        public async Task<bool> DeleteQuotationAsync(int id)
        {
            var quotation = await _context.Quotations.FindAsync(id);
            if (quotation == null) return false;

            _context.Quotations.Remove(quotation);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Quotation>> SearchQuotationsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllQuotationsAsync();

            searchTerm = searchTerm.ToLower();
            return await _context.Quotations
                .Include(q => q.Customer)
                .Include(q => q.LineItems)
                    .ThenInclude(li => li.Product)
                .Where(q => q.QuotationNumber.ToLower().Contains(searchTerm) ||
                           q.Customer.Name.ToLower().Contains(searchTerm) ||
                           (q.ProjectName != null && q.ProjectName.ToLower().Contains(searchTerm)) ||
                           (q.Description != null && q.Description.ToLower().Contains(searchTerm)))
                .OrderByDescending(q => q.CreatedDate)
                .ToListAsync();
        }

        public async Task<bool> QuotationExistsAsync(int id)
        {
            return await _context.Quotations.AnyAsync(q => q.Id == id);
        }

        public async Task<string> GenerateQuotationNumberAsync()
        {
            var year = DateTime.Now.Year;
            var month = DateTime.Now.Month;
            var prefix = $"QT{year:D4}{month:D2}";
            
            var lastQuotation = await _context.Quotations
                .Where(q => q.QuotationNumber.StartsWith(prefix))
                .OrderByDescending(q => q.QuotationNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastQuotation != null)
            {
                var lastNumberPart = lastQuotation.QuotationNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberPart, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
        }

        // Line item methods
        public async Task<QuotationLineItem> AddLineItemAsync(QuotationLineItem lineItem)
        {
            lineItem.LineTotal = lineItem.Quantity * lineItem.UnitPrice;
            _context.QuotationLineItems.Add(lineItem);
            await _context.SaveChangesAsync();
            
            await UpdateQuotationTotalsAsync(lineItem.QuotationId);
            return lineItem;
        }

        public async Task<QuotationLineItem> UpdateLineItemAsync(QuotationLineItem lineItem)
        {
            lineItem.LineTotal = lineItem.Quantity * lineItem.UnitPrice;
            _context.Entry(lineItem).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            await UpdateQuotationTotalsAsync(lineItem.QuotationId);
            return lineItem;
        }

        public async Task<bool> DeleteLineItemAsync(int lineItemId)
        {
            var lineItem = await _context.QuotationLineItems.FindAsync(lineItemId);
            if (lineItem == null) return false;

            var quotationId = lineItem.QuotationId;
            _context.QuotationLineItems.Remove(lineItem);
            await _context.SaveChangesAsync();
            
            await UpdateQuotationTotalsAsync(quotationId);
            return true;
        }

        public async Task<bool> UpdateQuotationTotalsAsync(int quotationId)
        {
            var quotation = await _context.Quotations
                .Include(q => q.LineItems)
                .FirstOrDefaultAsync(q => q.Id == quotationId);

            if (quotation == null) return false;

            var totals = _taxCalculationService.CalculateQuotationTotals(
                quotation.LineItems, quotation.TaxRate, quotation.IsTaxInclusive);

            quotation.SubTotal = totals.subTotal;
            quotation.TaxAmount = totals.taxAmount;
            quotation.TotalAmount = totals.totalAmount;
            quotation.UpdatedDate = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        // Status management
        public async Task<bool> UpdateQuotationStatusAsync(int quotationId, QuotationStatus status)
        {
            var quotation = await _context.Quotations.FindAsync(quotationId);
            if (quotation == null) return false;

            quotation.Status = status;
            quotation.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Quotation>> GetExpiredQuotationsAsync()
        {
            var today = DateTime.Today;
            return await _context.Quotations
                .Include(q => q.Customer)
                .Where(q => q.ValidUntil.HasValue && 
                           q.ValidUntil.Value < today && 
                           q.Status != QuotationStatus.Expired &&
                           q.Status != QuotationStatus.Converted &&
                           q.Status != QuotationStatus.Rejected)
                .ToListAsync();
        }
    }
}
