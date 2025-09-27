using InvGen.Web.Models;
using Microsoft.EntityFrameworkCore;

namespace InvGen.Web.Data
{
    public class InvGenDbContext : DbContext
    {
        public InvGenDbContext(DbContextOptions<InvGenDbContext> options) : base(options)
        {
        }

        public DbSet<Customer> Customers { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Quotation> Quotations { get; set; }
        public DbSet<QuotationLineItem> QuotationLineItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure relationships
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Category)
                .WithMany(c => c.Products)
                .HasForeignKey(p => p.CategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Quotation>()
                .HasOne(q => q.Customer)
                .WithMany(c => c.Quotations)
                .HasForeignKey(q => q.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<QuotationLineItem>()
                .HasOne(qli => qli.Quotation)
                .WithMany(q => q.LineItems)
                .HasForeignKey(qli => qli.QuotationId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<QuotationLineItem>()
                .HasOne(qli => qli.Product)
                .WithMany(p => p.QuotationLineItems)
                .HasForeignKey(qli => qli.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure indexes
            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Product>()
                .HasIndex(p => p.Code)
                .IsUnique();

            modelBuilder.Entity<Quotation>()
                .HasIndex(q => q.QuotationNumber)
                .IsUnique();

            // Seed data
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed Product Categories
            modelBuilder.Entity<ProductCategory>().HasData(
                new ProductCategory { Id = 1, Name = "Plumbing Pipes", ServiceType = ServiceType.Plumbing, Description = "Various types of plumbing pipes" },
                new ProductCategory { Id = 2, Name = "Plumbing Fittings", ServiceType = ServiceType.Plumbing, Description = "Pipe fittings and connectors" },
                new ProductCategory { Id = 3, Name = "Plumbing Fixtures", ServiceType = ServiceType.Plumbing, Description = "Sinks, toilets, and other fixtures" },
                new ProductCategory { Id = 4, Name = "Electrical Wires", ServiceType = ServiceType.Electrical, Description = "Electrical wiring and cables" },
                new ProductCategory { Id = 5, Name = "Electrical Switches", ServiceType = ServiceType.Electrical, Description = "Switches and outlets" },
                new ProductCategory { Id = 6, Name = "Electrical Panels", ServiceType = ServiceType.Electrical, Description = "Distribution panels and breakers" },
                new ProductCategory { Id = 7, Name = "Labor - Plumbing", ServiceType = ServiceType.Plumbing, Description = "Plumbing labor and services" },
                new ProductCategory { Id = 8, Name = "Labor - Electrical", ServiceType = ServiceType.Electrical, Description = "Electrical labor and services" }
            );

            // Seed Products - Plumbing Pipes
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "PVC Pipe 1/2 inch", Code = "PVC-12", CategoryId = 1, UnitPrice = 45.00m, Unit = "meter", Brand = "Supreme", Description = "PVC pipe for water supply", IsService = false },
                new Product { Id = 2, Name = "PVC Pipe 3/4 inch", Code = "PVC-34", CategoryId = 1, UnitPrice = 65.00m, Unit = "meter", Brand = "Supreme", Description = "PVC pipe for water supply", IsService = false },
                new Product { Id = 3, Name = "PVC Pipe 1 inch", Code = "PVC-1", CategoryId = 1, UnitPrice = 85.00m, Unit = "meter", Brand = "Supreme", Description = "PVC pipe for water supply", IsService = false },
                new Product { Id = 4, Name = "CPVC Pipe 1/2 inch", Code = "CPVC-12", CategoryId = 1, UnitPrice = 75.00m, Unit = "meter", Brand = "Ashirvad", Description = "CPVC pipe for hot water", IsService = false },
                new Product { Id = 5, Name = "CPVC Pipe 3/4 inch", Code = "CPVC-34", CategoryId = 1, UnitPrice = 95.00m, Unit = "meter", Brand = "Ashirvad", Description = "CPVC pipe for hot water", IsService = false },

                // Plumbing Fittings
                new Product { Id = 6, Name = "PVC Elbow 1/2 inch", Code = "ELBOW-12", CategoryId = 2, UnitPrice = 15.00m, Unit = "piece", Brand = "Supreme", Description = "90 degree elbow fitting", IsService = false },
                new Product { Id = 7, Name = "PVC Tee 1/2 inch", Code = "TEE-12", CategoryId = 2, UnitPrice = 18.00m, Unit = "piece", Brand = "Supreme", Description = "T-junction fitting", IsService = false },
                new Product { Id = 8, Name = "PVC Reducer 3/4 to 1/2", Code = "RED-3412", CategoryId = 2, UnitPrice = 22.00m, Unit = "piece", Brand = "Supreme", Description = "Pipe size reducer", IsService = false },
                new Product { Id = 9, Name = "Ball Valve 1/2 inch", Code = "VALVE-12", CategoryId = 2, UnitPrice = 125.00m, Unit = "piece", Brand = "Jaquar", Description = "Brass ball valve", IsService = false },
                new Product { Id = 10, Name = "Gate Valve 3/4 inch", Code = "GATE-34", CategoryId = 2, UnitPrice = 185.00m, Unit = "piece", Brand = "Jaquar", Description = "Brass gate valve", IsService = false },

                // Plumbing Fixtures
                new Product { Id = 11, Name = "Kitchen Sink Single Bowl", Code = "SINK-K1", CategoryId = 3, UnitPrice = 3500.00m, Unit = "piece", Brand = "Nirali", Description = "Stainless steel kitchen sink", IsService = false },
                new Product { Id = 12, Name = "Wash Basin", Code = "BASIN-W1", CategoryId = 3, UnitPrice = 2800.00m, Unit = "piece", Brand = "Hindware", Description = "Ceramic wash basin", IsService = false },
                new Product { Id = 13, Name = "Water Closet", Code = "WC-1", CategoryId = 3, UnitPrice = 4500.00m, Unit = "piece", Brand = "Kohler", Description = "One piece toilet", IsService = false },
                new Product { Id = 14, Name = "Shower Head", Code = "SHOWER-1", CategoryId = 3, UnitPrice = 850.00m, Unit = "piece", Brand = "Jaquar", Description = "Rain shower head", IsService = false },
                new Product { Id = 15, Name = "Faucet Single Lever", Code = "TAP-1", CategoryId = 3, UnitPrice = 1250.00m, Unit = "piece", Brand = "Grohe", Description = "Single lever basin mixer", IsService = false },

                // Electrical Wires
                new Product { Id = 16, Name = "Copper Wire 2.5 sq mm", Code = "WIRE-25", CategoryId = 4, UnitPrice = 285.00m, Unit = "meter", Brand = "Polycab", Description = "Single core copper wire", IsService = false },
                new Product { Id = 17, Name = "Copper Wire 4 sq mm", Code = "WIRE-4", CategoryId = 4, UnitPrice = 425.00m, Unit = "meter", Brand = "Polycab", Description = "Single core copper wire", IsService = false },
                new Product { Id = 18, Name = "Copper Wire 6 sq mm", Code = "WIRE-6", CategoryId = 4, UnitPrice = 625.00m, Unit = "meter", Brand = "Polycab", Description = "Single core copper wire", IsService = false },
                new Product { Id = 19, Name = "3 Core Cable 2.5 sq mm", Code = "CABLE-3C25", CategoryId = 4, UnitPrice = 145.00m, Unit = "meter", Brand = "Havells", Description = "3 core flexible cable", IsService = false },
                new Product { Id = 20, Name = "Armoured Cable 4 sq mm", Code = "ACABLE-4", CategoryId = 4, UnitPrice = 185.00m, Unit = "meter", Brand = "KEI", Description = "Armoured power cable", IsService = false },

                // Electrical Switches
                new Product { Id = 21, Name = "Modular Switch 1 Way", Code = "SW-1W", CategoryId = 5, UnitPrice = 85.00m, Unit = "piece", Brand = "Legrand", Description = "1 way modular switch", IsService = false },
                new Product { Id = 22, Name = "Modular Switch 2 Way", Code = "SW-2W", CategoryId = 5, UnitPrice = 125.00m, Unit = "piece", Brand = "Legrand", Description = "2 way modular switch", IsService = false },
                new Product { Id = 23, Name = "Socket 3 Pin", Code = "SOCKET-3P", CategoryId = 5, UnitPrice = 95.00m, Unit = "piece", Brand = "Anchor", Description = "3 pin power socket", IsService = false },
                new Product { Id = 24, Name = "Socket 2 Pin", Code = "SOCKET-2P", CategoryId = 5, UnitPrice = 75.00m, Unit = "piece", Brand = "Anchor", Description = "2 pin power socket", IsService = false },
                new Product { Id = 25, Name = "Dimmer Switch", Code = "DIMMER-1", CategoryId = 5, UnitPrice = 285.00m, Unit = "piece", Brand = "Schneider", Description = "LED dimmer switch", IsService = false },

                // Electrical Panels
                new Product { Id = 26, Name = "MCB 16A Single Pole", Code = "MCB-16A", CategoryId = 6, UnitPrice = 185.00m, Unit = "piece", Brand = "Schneider", Description = "16A miniature circuit breaker", IsService = false },
                new Product { Id = 27, Name = "MCB 32A Single Pole", Code = "MCB-32A", CategoryId = 6, UnitPrice = 225.00m, Unit = "piece", Brand = "Schneider", Description = "32A miniature circuit breaker", IsService = false },
                new Product { Id = 28, Name = "RCCB 30mA 40A", Code = "RCCB-40A", CategoryId = 6, UnitPrice = 1850.00m, Unit = "piece", Brand = "Siemens", Description = "Residual current circuit breaker", IsService = false },
                new Product { Id = 29, Name = "Distribution Board 8 Way", Code = "DB-8W", CategoryId = 6, UnitPrice = 1250.00m, Unit = "piece", Brand = "Legrand", Description = "8 way distribution board", IsService = false },
                new Product { Id = 30, Name = "Distribution Board 12 Way", Code = "DB-12W", CategoryId = 6, UnitPrice = 1650.00m, Unit = "piece", Brand = "Legrand", Description = "12 way distribution board", IsService = false },

                // Labor Services - Plumbing
                new Product { Id = 31, Name = "Plumbing Installation", Code = "LAB-P-INST", CategoryId = 7, UnitPrice = 500.00m, Unit = "hour", Description = "General plumbing installation work", IsService = true },
                new Product { Id = 32, Name = "Pipe Laying", Code = "LAB-P-PIPE", CategoryId = 7, UnitPrice = 45.00m, Unit = "meter", Description = "Pipe laying and connection", IsService = true },
                new Product { Id = 33, Name = "Fixture Installation", Code = "LAB-P-FIX", CategoryId = 7, UnitPrice = 750.00m, Unit = "piece", Description = "Bathroom fixture installation", IsService = true },
                new Product { Id = 34, Name = "Leak Repair", Code = "LAB-P-LEAK", CategoryId = 7, UnitPrice = 350.00m, Unit = "hour", Description = "Leak detection and repair", IsService = true },

                // Labor Services - Electrical
                new Product { Id = 35, Name = "Electrical Installation", Code = "LAB-E-INST", CategoryId = 8, UnitPrice = 600.00m, Unit = "hour", Description = "General electrical installation work", IsService = true },
                new Product { Id = 36, Name = "Wiring Work", Code = "LAB-E-WIRE", CategoryId = 8, UnitPrice = 25.00m, Unit = "meter", Description = "Electrical wiring installation", IsService = true },
                new Product { Id = 37, Name = "Panel Installation", Code = "LAB-E-PANEL", CategoryId = 8, UnitPrice = 1200.00m, Unit = "piece", Description = "Electrical panel installation", IsService = true },
                new Product { Id = 38, Name = "Switch/Socket Installation", Code = "LAB-E-SW", CategoryId = 8, UnitPrice = 85.00m, Unit = "piece", Description = "Switch and socket installation", IsService = true }
            );
        }
    }
}
