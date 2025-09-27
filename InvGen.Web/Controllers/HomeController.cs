using InvGen.Web.Models;
using InvGen.Web.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace InvGen.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly IQuotationService _quotationService;
        private readonly ICustomerService _customerService;
        private readonly IProductService _productService;

        public HomeController(
            IQuotationService quotationService,
            ICustomerService customerService,
            IProductService productService)
        {
            _quotationService = quotationService;
            _customerService = customerService;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            var allQuotations = await _quotationService.GetAllQuotationsAsync();
            var allCustomers = await _customerService.GetAllCustomersAsync();
            var allProducts = await _productService.GetAllProductsAsync();

            var draftQuotations = allQuotations.Where(q => q.Status == QuotationStatus.Draft).ToList();
            var sentQuotations = allQuotations.Where(q => q.Status == QuotationStatus.Sent).ToList();
            var approvedQuotations = allQuotations.Where(q => q.Status == QuotationStatus.Approved).ToList();
            var rejectedQuotations = allQuotations.Where(q => q.Status == QuotationStatus.Rejected).ToList();

            // Calculate revenue metrics
            var totalRevenue = approvedQuotations.Sum(q => q.TotalAmount);
            var pendingRevenue = sentQuotations.Sum(q => q.TotalAmount);
            var thisMonthQuotations = allQuotations.Where(q => q.QuotationDate.Month == DateTime.Now.Month && q.QuotationDate.Year == DateTime.Now.Year).ToList();
            var thisMonthRevenue = thisMonthQuotations.Where(q => q.Status == QuotationStatus.Approved).Sum(q => q.TotalAmount);

            var model = new DashboardViewModel
            {
                RecentQuotations = allQuotations.OrderByDescending(q => q.QuotationDate).Take(10),
                TotalCustomers = allCustomers.Count(),
                TotalProducts = allProducts.Count(),
                DraftQuotations = draftQuotations.Count,
                SentQuotations = sentQuotations.Count,
                ApprovedQuotations = approvedQuotations.Count,
                RejectedQuotations = rejectedQuotations.Count,
                TotalQuotations = allQuotations.Count(),
                TotalRevenue = totalRevenue,
                PendingRevenue = pendingRevenue,
                ThisMonthRevenue = thisMonthRevenue,
                ThisMonthQuotations = thisMonthQuotations.Count,
                ActiveCustomers = allCustomers.Count(c => c.IsActive),
                PlumbingProducts = allProducts.Count(p => p.Category.ServiceType == ServiceType.Plumbing),
                ElectricalProducts = allProducts.Count(p => p.Category.ServiceType == ServiceType.Electrical),
                ConversionRate = allQuotations.Any() ? (double)approvedQuotations.Count / allQuotations.Count() * 100 : 0
            };

            return View(model);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Test()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    public class DashboardViewModel
    {
        public IEnumerable<Quotation> RecentQuotations { get; set; } = new List<Quotation>();
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public int DraftQuotations { get; set; }
        public int SentQuotations { get; set; }
        public int ApprovedQuotations { get; set; }
        public int RejectedQuotations { get; set; }
        public int TotalQuotations { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal PendingRevenue { get; set; }
        public decimal ThisMonthRevenue { get; set; }
        public int ThisMonthQuotations { get; set; }
        public int ActiveCustomers { get; set; }
        public int PlumbingProducts { get; set; }
        public int ElectricalProducts { get; set; }
        public double ConversionRate { get; set; }
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
