using InvGen.Web.Models;
using InvGen.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvGen.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersApiController : ControllerBase
    {
        private readonly ICustomerService _customerService;

        public CustomersApiController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCustomers(string? search)
        {
            IEnumerable<Customer> customers;

            if (!string.IsNullOrEmpty(search))
            {
                customers = await _customerService.SearchCustomersAsync(search);
            }
            else
            {
                customers = await _customerService.GetAllCustomersAsync();
            }

            var result = customers.Select(c => new
            {
                c.Id,
                c.Name,
                c.CompanyName,
                c.Email,
                c.Phone,
                c.Mobile,
                c.Address,
                c.City,
                c.State,
                c.PostalCode,
                c.Country,
                c.GstNumber,
                c.CreatedDate,
                QuotationCount = c.Quotations.Count
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetCustomer(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var result = new
            {
                customer.Id,
                customer.Name,
                customer.CompanyName,
                customer.Email,
                customer.Phone,
                customer.Mobile,
                customer.Address,
                customer.City,
                customer.State,
                customer.PostalCode,
                customer.Country,
                customer.GstNumber,
                customer.CreatedDate,
                customer.UpdatedDate,
                Quotations = customer.Quotations.Select(q => new
                {
                    q.Id,
                    q.QuotationNumber,
                    q.QuotationDate,
                    q.Status,
                    q.TotalAmount,
                    q.ProjectName
                }).OrderByDescending(q => q.QuotationDate)
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateCustomer(Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if email already exists
            if (await _customerService.EmailExistsAsync(customer.Email))
            {
                return BadRequest(new { error = "A customer with this email already exists." });
            }

            try
            {
                var createdCustomer = await _customerService.CreateCustomerAsync(customer);
                return CreatedAtAction(nameof(GetCustomer), new { id = createdCustomer.Id }, new { id = createdCustomer.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, Customer customer)
        {
            if (id != customer.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Check if email already exists for another customer
            if (await _customerService.EmailExistsAsync(customer.Email, customer.Id))
            {
                return BadRequest(new { error = "A customer with this email already exists." });
            }

            try
            {
                await _customerService.UpdateCustomerAsync(customer);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var exists = await _customerService.CustomerExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            try
            {
                await _customerService.DeleteCustomerAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<object>>> SearchCustomers(string term)
        {
            var customers = await _customerService.SearchCustomersAsync(term);
            var result = customers.Select(c => new
            {
                c.Id,
                c.Name,
                c.Email,
                c.CompanyName,
                c.Phone
            });

            return Ok(result);
        }

        [HttpGet("{id}/quotations")]
        public async Task<ActionResult<IEnumerable<object>>> GetCustomerQuotations(int id)
        {
            var customer = await _customerService.GetCustomerByIdAsync(id);
            if (customer == null)
            {
                return NotFound();
            }

            var result = customer.Quotations.Select(q => new
            {
                q.Id,
                q.QuotationNumber,
                q.QuotationDate,
                q.ValidUntil,
                q.Status,
                q.ProjectName,
                q.Description,
                q.SubTotal,
                q.TaxAmount,
                q.TotalAmount,
                LineItemCount = q.LineItems.Count
            }).OrderByDescending(q => q.QuotationDate);

            return Ok(result);
        }

        [HttpGet("validate-email")]
        public async Task<ActionResult<object>> ValidateEmail(string email, int? excludeId)
        {
            var exists = await _customerService.EmailExistsAsync(email, excludeId);
            return Ok(new { exists, available = !exists });
        }
    }
}
