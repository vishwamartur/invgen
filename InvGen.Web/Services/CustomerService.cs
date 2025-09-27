using InvGen.Web.Data;
using InvGen.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace InvGen.Web.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly InvGenDbContext _context;

        public CustomerService(InvGenDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .Where(c => c.IsActive)
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<Customer?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .Include(c => c.Quotations)
                .FirstOrDefaultAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<Customer> CreateCustomerAsync(Customer customer)
        {
            customer.CreatedDate = DateTime.UtcNow;
            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer> UpdateCustomerAsync(Customer customer)
        {
            customer.UpdatedDate = DateTime.UtcNow;
            _context.Entry(customer).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return false;

            // Soft delete
            customer.IsActive = false;
            customer.UpdatedDate = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Customer>> SearchCustomersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllCustomersAsync();

            searchTerm = searchTerm.ToLower();
            return await _context.Customers
                .Where(c => c.IsActive && 
                    (c.Name.ToLower().Contains(searchTerm) ||
                     c.Email.ToLower().Contains(searchTerm) ||
                     (c.CompanyName != null && c.CompanyName.ToLower().Contains(searchTerm)) ||
                     (c.Phone != null && c.Phone.Contains(searchTerm))))
                .OrderBy(c => c.Name)
                .ToListAsync();
        }

        public async Task<bool> CustomerExistsAsync(int id)
        {
            return await _context.Customers.AnyAsync(c => c.Id == id && c.IsActive);
        }

        public async Task<bool> EmailExistsAsync(string email, int? excludeId = null)
        {
            var query = _context.Customers.Where(c => c.Email == email && c.IsActive);
            if (excludeId.HasValue)
                query = query.Where(c => c.Id != excludeId.Value);
            
            return await query.AnyAsync();
        }
    }
}
