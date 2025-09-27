using InvGen.Web.Models;
using InvGen.Web.Services;
using Microsoft.AspNetCore.Mvc;

namespace InvGen.Web.Controllers.Api
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuotationsApiController : ControllerBase
    {
        private readonly IQuotationService _quotationService;
        private readonly IPdfService _pdfService;

        public QuotationsApiController(IQuotationService quotationService, IPdfService pdfService)
        {
            _quotationService = quotationService;
            _pdfService = pdfService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetQuotations(string? search, QuotationStatus? status)
        {
            IEnumerable<Quotation> quotations;

            if (!string.IsNullOrEmpty(search))
            {
                quotations = await _quotationService.SearchQuotationsAsync(search);
            }
            else if (status.HasValue)
            {
                quotations = await _quotationService.GetQuotationsByStatusAsync(status.Value);
            }
            else
            {
                quotations = await _quotationService.GetAllQuotationsAsync();
            }

            var result = quotations.Select(q => new
            {
                q.Id,
                q.QuotationNumber,
                q.QuotationDate,
                q.ValidUntil,
                q.Status,
                q.ProjectName,
                q.SubTotal,
                q.TaxAmount,
                q.TotalAmount,
                Customer = new
                {
                    q.Customer.Id,
                    q.Customer.Name,
                    q.Customer.Email,
                    q.Customer.CompanyName
                },
                LineItemCount = q.LineItems.Count
            });

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetQuotation(int id)
        {
            var quotation = await _quotationService.GetQuotationByIdAsync(id);
            if (quotation == null)
            {
                return NotFound();
            }

            var result = new
            {
                quotation.Id,
                quotation.QuotationNumber,
                quotation.QuotationDate,
                quotation.ValidUntil,
                quotation.Status,
                quotation.ProjectName,
                quotation.Description,
                quotation.Notes,
                quotation.TermsAndConditions,
                quotation.SubTotal,
                quotation.TaxRate,
                quotation.TaxAmount,
                quotation.TotalAmount,
                quotation.IsTaxInclusive,
                Customer = new
                {
                    quotation.Customer.Id,
                    quotation.Customer.Name,
                    quotation.Customer.Email,
                    quotation.Customer.CompanyName,
                    quotation.Customer.Phone,
                    quotation.Customer.Address
                },
                LineItems = quotation.LineItems.Select(li => new
                {
                    li.Id,
                    li.Description,
                    li.Quantity,
                    li.Unit,
                    li.UnitPrice,
                    li.LineTotal,
                    li.SortOrder,
                    Product = new
                    {
                        li.Product.Id,
                        li.Product.Name,
                        li.Product.Code,
                        li.Product.IsService,
                        Category = li.Product.Category.Name
                    }
                }).OrderBy(li => li.SortOrder)
            };

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<object>> CreateQuotation(Quotation quotation)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdQuotation = await _quotationService.CreateQuotationAsync(quotation);
                return CreatedAtAction(nameof(GetQuotation), new { id = createdQuotation.Id }, new { id = createdQuotation.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuotation(int id, Quotation quotation)
        {
            if (id != quotation.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _quotationService.UpdateQuotationAsync(quotation);
                await _quotationService.UpdateQuotationTotalsAsync(quotation.Id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuotation(int id)
        {
            var exists = await _quotationService.QuotationExistsAsync(id);
            if (!exists)
            {
                return NotFound();
            }

            try
            {
                await _quotationService.DeleteQuotationAsync(id);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPatch("{id}/status")]
        public async Task<IActionResult> UpdateQuotationStatus(int id, [FromBody] QuotationStatus status)
        {
            try
            {
                var success = await _quotationService.UpdateQuotationStatusAsync(id, status);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("{id}/line-items")]
        public async Task<ActionResult<object>> AddLineItem(int id, QuotationLineItem lineItem)
        {
            lineItem.QuotationId = id;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                var createdLineItem = await _quotationService.AddLineItemAsync(lineItem);
                return Ok(new { id = createdLineItem.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPut("line-items/{lineItemId}")]
        public async Task<IActionResult> UpdateLineItem(int lineItemId, QuotationLineItem lineItem)
        {
            if (lineItemId != lineItem.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                await _quotationService.UpdateLineItemAsync(lineItem);
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("line-items/{lineItemId}")]
        public async Task<IActionResult> DeleteLineItem(int lineItemId)
        {
            try
            {
                var success = await _quotationService.DeleteLineItemAsync(lineItemId);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}/pdf")]
        public async Task<IActionResult> GeneratePdf(int id)
        {
            try
            {
                var pdfBytes = await _pdfService.GenerateQuotationPdfAsync(id);
                var quotation = await _quotationService.GetQuotationByIdAsync(id);
                var fileName = $"Quotation_{quotation?.QuotationNumber}_{DateTime.Now:yyyyMMdd}.pdf";
                
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("generate-number")]
        public async Task<ActionResult<object>> GenerateQuotationNumber()
        {
            try
            {
                var quotationNumber = await _quotationService.GenerateQuotationNumberAsync();
                return Ok(new { quotationNumber });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
