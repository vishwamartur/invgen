using System.ComponentModel.DataAnnotations;

namespace InvGen.Web.Models
{
    public class ProductCategory
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Description { get; set; }

        [Required]
        public ServiceType ServiceType { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }

    public enum ServiceType
    {
        Plumbing = 1,
        Electrical = 2,
        Both = 3
    }
}
