using InvGen.Web.Models;

namespace InvGen.Web.Services
{
    public interface ITaxCalculationService
    {
        decimal CalculateSubTotal(IEnumerable<QuotationLineItem> lineItems);
        decimal CalculateTaxAmount(decimal subTotal, decimal taxRate, bool isTaxInclusive);
        decimal CalculateTotalAmount(decimal subTotal, decimal taxAmount, bool isTaxInclusive);
        (decimal subTotal, decimal taxAmount, decimal totalAmount) CalculateQuotationTotals(
            IEnumerable<QuotationLineItem> lineItems, decimal taxRate, bool isTaxInclusive);
    }

    public class TaxCalculationService : ITaxCalculationService
    {
        public decimal CalculateSubTotal(IEnumerable<QuotationLineItem> lineItems)
        {
            return lineItems.Sum(item => item.Quantity * item.UnitPrice);
        }

        public decimal CalculateTaxAmount(decimal subTotal, decimal taxRate, bool isTaxInclusive)
        {
            if (isTaxInclusive)
            {
                // Tax amount = (SubTotal * TaxRate) / (100 + TaxRate)
                return Math.Round((subTotal * taxRate) / (100 + taxRate), 2);
            }
            else
            {
                // Tax amount = (SubTotal * TaxRate) / 100
                return Math.Round((subTotal * taxRate) / 100, 2);
            }
        }

        public decimal CalculateTotalAmount(decimal subTotal, decimal taxAmount, bool isTaxInclusive)
        {
            if (isTaxInclusive)
            {
                return subTotal; // SubTotal already includes tax
            }
            else
            {
                return subTotal + taxAmount;
            }
        }

        public (decimal subTotal, decimal taxAmount, decimal totalAmount) CalculateQuotationTotals(
            IEnumerable<QuotationLineItem> lineItems, decimal taxRate, bool isTaxInclusive)
        {
            var subTotal = CalculateSubTotal(lineItems);
            var taxAmount = CalculateTaxAmount(subTotal, taxRate, isTaxInclusive);
            var totalAmount = CalculateTotalAmount(subTotal, taxAmount, isTaxInclusive);

            return (subTotal, taxAmount, totalAmount);
        }
    }
}
