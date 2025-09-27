using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace InvGen.Web.Models
{
    public class Quotation
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string QuotationNumber { get; set; } = string.Empty;

        [Required]
        public int CustomerId { get; set; }

        [Required]
        public DateTime QuotationDate { get; set; } = DateTime.UtcNow;

        public DateTime? ValidUntil { get; set; }

        [Required]
        public QuotationStatus Status { get; set; } = QuotationStatus.Draft;

        [StringLength(100)]
        public string? ProjectName { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }

        [StringLength(1000)]
        public string? TermsAndConditions { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal SubTotal { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal TaxRate { get; set; } = 18.0m; // GST rate

        [Column(TypeName = "decimal(18,2)")]
        public decimal TaxAmount { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        public bool IsTaxInclusive { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }

        [StringLength(100)]
        public string? CreatedBy { get; set; }

        // Navigation properties
        [ForeignKey("CustomerId")]
        public virtual Customer Customer { get; set; } = null!;

        public virtual ICollection<QuotationLineItem> LineItems { get; set; } = new List<QuotationLineItem>();
    }

    public enum QuotationStatus
    {
        Draft = 1,
        Sent = 2,
        Approved = 3,
        Rejected = 4,
        Expired = 5,
        Converted = 6
    }
}
