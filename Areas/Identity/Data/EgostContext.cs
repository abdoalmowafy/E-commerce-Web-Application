using Egost.Areas.Identity.Data;
using Egost.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Drawing;

namespace Egost.Data;

public class EgostContext(DbContextOptions<EgostContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<EditHistory> EditHistories { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartProduct> CartProducts { get; set; }
    public DbSet<PromoCode> PromoCodes { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<ReturnProductOrder> ReturnProductOrders { get; set; }
    public DbSet<Search> Searches { get; set; }


    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Add new cart for new users
        var UserEntries = ChangeTracker.Entries<User>();

        var AddedUserEntries = UserEntries.Where(e => e.State == EntityState.Added).ToList();
        foreach (var entry in AddedUserEntries)
        {
            var user = entry.Entity;
            if (user.Cart == null)
            {
                Cart cart = new();
                Carts.Add(cart);
                user.Cart = cart;
            }
        }

        var DeletedUserEntries = UserEntries.Where(e => e.State == EntityState.Deleted).ToList();
        foreach (var entry in DeletedUserEntries)
        {
            var user = entry.Entity;

            var UserOrders = user.Orders;
            foreach (var order in UserOrders)
            {
                if ( order.DeletedDateTime == null && order.DeliveryDateTime == null 
                    && (order.PaymentMethod == "COD" || !order.Processed))
                {
                    foreach (var orderProduct in order.OrderProducts)
                    {
                        orderProduct.Product.SKU += orderProduct.Quantity;
                        Products.Update(orderProduct.Product);
                    }
                    order.DeletedDateTime = DateTime.Now;
                }
                order.User = null;
                Orders.Update(order);
            }

            var UserEdits = user.EditsHistory;
            if (UserEdits != null)
            {
                foreach (var edit in UserEdits)
                {
                    EditHistories.Remove(edit);
                }
            }

            var cart = user.Cart;
            var cartProducts = cart.CartProducts;
            if (cartProducts != null)
            {
                foreach (var cp in cartProducts)
                {
                    CartProducts.Remove(cp);
                }
            }
            Carts.Remove(cart);
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    public int SaveChanges(User currentUser)
    {
        var entries = ChangeTracker.Entries().ToList();
        var editedEntries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified).ToList();

        foreach (var entry in editedEntries)
        {
            var editedObj = entry.Entity;
            var entityType = editedObj.GetType();
            var editsHistoryProperty = entityType.GetProperty("EditsHistory");

            if (editsHistoryProperty != null)
            {
                // Get the EditsHistory collection
                var editsHistory = (ICollection<EditHistory>)editsHistoryProperty.GetValue(editedObj);

                foreach (var prop in entry.Properties.Where(p => !Equals(p.OriginalValue, p.CurrentValue) && IsSimpleType(p.Metadata.ClrType)).ToList())
                {
                    var edit = new EditHistory
                    {
                        Editor = currentUser,
                        Field = prop.Metadata.Name,
                        OldData = prop.OriginalValue?.ToString(),
                        NewData = prop.CurrentValue?.ToString()
                    };

                    editsHistory.Add(edit);
                    EditHistories.Add(edit);
                }
            }
        }

        return base.SaveChanges();
    }

    private static bool IsSimpleType(Type type) =>  type.IsPrimitive || // For basic types like int, byte, etc.
                                                    type == typeof(string) ||
                                                    type == typeof(decimal) ||
                                                    type == typeof(DateTime) ||
                                                    type == typeof(Guid) ||
                                                    type == typeof(TimeSpan) ||
                                                    type == typeof(DateTimeOffset) ||
                                                    type.IsEnum;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        Seed(builder);

        // ---User relations---
        // One User has many Addresses
        builder.Entity<User>()
            .HasMany(u => u.Addresses)
            .WithOne();

        // Many Users has many product wishlist
        builder.Entity<User>()
            .HasMany(u => u.WishList)
            .WithMany(p => p.WishlistUsers);

        // One User has many Orders
        builder.Entity<User>()
            .HasMany(u => u.Orders)
            .WithOne(o => o.User)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // One User has many EditsHistory
        builder.Entity<User>()
            .HasMany(u => u.EditsHistory)
            .WithOne()
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);



        // ---Address relations---
        // One Address has many EditsHistory
        builder.Entity<Address>()
            .HasMany(address => address.EditsHistory)
            .WithOne()
            .OnDelete(DeleteBehavior.Restrict);



        // --- Cart relations---
        // One Cart has many CartProducts
        builder.Entity<Cart>()
            .HasMany(c => c.CartProducts)
            .WithOne();

        // One PromoCode has many Carts
        builder.Entity<Cart>()
            .HasOne(c => c.PromoCode)
            .WithMany();



        // --- CartProduct relations---
        // One Product has many CartProducts
        builder.Entity<CartProduct>()
            .HasOne(cp => cp.Product)
            .WithMany();



        // --- Order relations
        // One Transporter has many Orders
        builder.Entity<Order>()
            .HasOne(o => o.Transporter)
            .WithMany()
            .HasForeignKey(o => o.TransporterId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Order has many OrderProducts
        builder.Entity<Order>()
            .HasMany(o => o.OrderProducts)
            .WithOne();

        // One PromoCode has many Orders
        builder.Entity<Order>()
            .HasOne(o => o.PromoCode)
            .WithMany();

        // Many Orders has One Address
        builder.Entity<Order>()
            .HasOne(o => o.Address)
            .WithMany();



        // ---OrderProduct relations---
        // One Product has Many OrderProducts
        builder.Entity<OrderProduct>()
            .HasOne(op => op.Product)
            .WithMany();



        // ---Product relations---
        // One Category has many Products
        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Product has many Reviews
        builder.Entity<Product>()
            .HasMany(p => p.Reviews)
            .WithOne(r => r.Product)
            .HasForeignKey(r => r.ProductId)
            .OnDelete(DeleteBehavior.Restrict);

        // One Product has many EditsHistory
        builder.Entity<Product>()
            .HasMany(p => p.EditsHistory)
            .WithOne()
            .OnDelete(DeleteBehavior.Restrict);



        // ---PromoCode relations---
        // One PromoCode has many EditHistories
        builder.Entity<PromoCode>()
            .HasMany(pc => pc.EditsHistory)
            .WithOne()
            .OnDelete(DeleteBehavior.Restrict);



        // ---ReturnProductOrder relations---
        // One Transporter has many ReturnProductOrders
        builder.Entity<ReturnProductOrder>()
            .HasOne(rpo => rpo.Transporter)
            .WithMany();

        // One Order has many ReturnProductOrders
        builder.Entity<ReturnProductOrder>()
            .HasOne(rpo => rpo.Order)
            .WithMany();

        // One OrderProduct has many ReturnProductOrders
        builder.Entity<ReturnProductOrder>()
            .HasOne(rpo => rpo.OrderProduct)
            .WithMany();



        // ---Review relations---
        // One Reviewer has many Reviews
        builder.Entity<Review>()
            .HasOne(rev => rev.Reviewer)
            .WithMany();

        // One Review has many EditHistories
        builder.Entity<Review>()
            .HasMany(r => r.EditsHistory)
            .WithOne();



        // ---Search relations---
        // One User has many Searches
        builder.Entity<Search>()
            .HasOne(s => s.User)
            .WithMany();

        // One Category has many Searches
        builder.Entity<Search>()
            .HasOne(s => s.Category)
            .WithMany();
    }

    private static void Seed(ModelBuilder builder)
    {
        // Seed Base Address
        builder.Entity<Address>().HasData(Address.Base);

        // Seed Categories
        builder.Entity<Category>().HasData(
            new Category { Id = 1, Name = "Sports, Instruments & Accessories" },
            new Category { Id = 2, Name = "Toys, Games, Video Games & Accessories" },
            new Category { Id = 3, Name = "Arts, Crafts & Sewing" },
            new Category { Id = 4, Name = "Clothing, Shoes & Jewelry" },
            new Category { Id = 5, Name = "Beauty & Personal Care" },
            new Category { Id = 6, Name = "Books" },
            new Category { Id = 7, Name = "Electronics & Accessories" },
            new Category { Id = 8, Name = "Software" },
            new Category { Id = 9, Name = "Grocery & Gourmet Food" },
            new Category { Id = 10, Name = "Home Furniture & Accessories" },
            new Category { Id = 11, Name = "Luggage & Travel Gear" },
            new Category { Id = 12, Name = "Pet Supplies" }
        );

        // Seed Products
        builder.Entity<Product>().HasData(
            // Sports, Instruments & Accessories
            new Product
            {
                Id = 1,
                Name = "Wilson Tennis Racket",
                Description = "High-quality tennis racket for professionals.",
                CategoryId = 1,
                SKU = 10001,
                PriceCents = 8999,
                SalePercent = 10,
                Warranty = new(730, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 2,
                Name = "Yamaha Acoustic Guitar",
                Description = "Top-notch acoustic guitar with a smooth finish.",
                CategoryId = 1,
                SKU = 10002,
                PriceCents = 14999,
                SalePercent = 15,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 3,
                Name = "EA sports FC24 for PS5",
                Description = "Latest EA sports soccer game ps5 edition.",
                CategoryId = 1,
                SKU = 10003,
                PriceCents = 12999,
                SalePercent = 5,
                Warranty = new(14, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 4,
                Name = "Adidas Soccer Ball",
                Description = "Official size soccer ball for all levels.",
                CategoryId = 1,
                SKU = 10004,
                PriceCents = 2999,
                SalePercent = 0,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 5,
                Name = "Wilson Badminton Set",
                Description = "Complete badminton set for backyard fun.",
                CategoryId = 1,
                SKU = 10005,
                PriceCents = 4599,
                SalePercent = 0,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },

            // Toys, Games, Video Games & Accessories
            new Product
            {
                Id = 6,
                Name = "LEGO Star Wars Set",
                Description = "Buildable Star Wars-themed LEGO set.",
                CategoryId = 2,
                SKU = 20001,
                PriceCents = 7999,
                SalePercent = 5,
                Warranty = new(183, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 7,
                Name = "PlayStation 5 Console",
                Description = "Next-generation gaming console with ultra-high-speed SSD.",
                CategoryId = 2,
                SKU = 20002,
                PriceCents = 49999,
                SalePercent = 0,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 8,
                Name = "Xbox Series X",
                Description = "Powerful gaming console with immersive gameplay.",
                CategoryId = 2,
                SKU = 20003,
                PriceCents = 49999,
                SalePercent = 0,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 9,
                Name = "Nintendo Switch",
                Description = "Portable gaming console for versatile play.",
                CategoryId = 2,
                SKU = 20004,
                PriceCents = 29999,
                SalePercent = 0,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 10,
                Name = "Hasbro Monopoly Game",
                Description = "Classic board game for family and friends.",
                CategoryId = 2,
                SKU = 20005,
                PriceCents = 1999,
                SalePercent = 0,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },

            // Arts, Crafts & Sewing
            new Product
            {
                Id = 11,
                Name = "Singer Sewing Machine",
                Description = "Reliable sewing machine for all skill levels.",
                CategoryId = 3,
                SKU = 30001,
                PriceCents = 15999,
                SalePercent = 20,
                Warranty = new(1095, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 12,
                Name = "Cricut Maker Machine",
                Description = "Versatile cutting machine for crafting projects.",
                CategoryId = 3,
                SKU = 30002,
                PriceCents = 39999,
                SalePercent = 10,
                Warranty = new(730, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 13,
                Name = "Faber-Castell Colored Pencils",
                Description = "High-quality colored pencils for artists.",
                CategoryId = 3,
                SKU = 30003,
                PriceCents = 2499,
                SalePercent = 5,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 14,
                Name = "Prismacolor Markers",
                Description = "Alcohol-based markers for smooth blending.",
                CategoryId = 3,
                SKU = 30004,
                PriceCents = 3999,
                SalePercent = 10,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 15,
                Name = "Schmincke Watercolors",
                Description = "Premium watercolor paints for artists.",
                CategoryId = 3,
                SKU = 30005,
                PriceCents = 5999,
                SalePercent = 5,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },

            // Clothing, Shoes & Jewelry
            new Product
            {
                Id = 16,
                Name = "Levi's Denim Jeans",
                Description = "Classic straight-fit jeans for men.",
                CategoryId = 4,
                SKU = 40001,
                PriceCents = 4999,
                SalePercent = 10,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 17,
                Name = "Nike Air Max Sneakers",
                Description = "Comfortable and stylish sneakers for daily wear.",
                CategoryId = 4,
                SKU = 40002,
                PriceCents = 8999,
                SalePercent = 15,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 18,
                Name = "Calvin Klein T-shirt",
                Description = "Soft cotton T-shirt with modern fit.",
                CategoryId = 4,
                SKU = 40003,
                PriceCents = 1999,
                SalePercent = 0,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 19,
                Name = "Ray-Ban Aviator Sunglasses",
                Description = "Iconic sunglasses with a timeless design.",
                CategoryId = 4,
                SKU = 40004,
                PriceCents = 14999,
                SalePercent = 10,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 20,
                Name = "Michael Kors Leather Handbag",
                Description = "Luxury leather handbag with modern style.",
                CategoryId = 4,
                SKU = 40005,
                PriceCents = 29999,
                SalePercent = 5,
                Warranty = new(730, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },

            // Beauty & Personal Care
            new Product
            {
                Id = 21,
                Name = "Revlon Hair Dryer",
                Description = "Powerful hair dryer with multiple heat settings.",
                CategoryId = 5,
                SKU = 50001,
                PriceCents = 3999,
                SalePercent = 10,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 22,
                Name = "Olay Regenerist Cream",
                Description = "Anti-aging cream for daily use.",
                CategoryId = 5,
                SKU = 50002,
                PriceCents = 2999,
                SalePercent = 5,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 23,
                Name = "Philips Electric Shaver",
                Description = "Cordless electric shaver with precision blades.",
                CategoryId = 5,
                SKU = 50003,
                PriceCents = 7999,
                SalePercent = 15,
                Warranty = new(730, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 24,
                Name = "Oral-B Electric Toothbrush",
                Description = "Rechargeable toothbrush with multiple brush heads.",
                CategoryId = 5,
                SKU = 50004,
                PriceCents = 5999,
                SalePercent = 10,
                Warranty = new(730, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            },
            new Product
            {
                Id = 25,
                Name = "Dove Body Wash",
                Description = "Moisturizing body wash for soft skin.",
                CategoryId = 5,
                SKU = 50005,
                PriceCents = 1299,
                SalePercent = 0,
                Warranty = new(365, 0, 0, 0),
                CreatedDateTime = DateTime.Now
            });

        // Seed PromoCodes
        builder.Entity<PromoCode>().HasData(
            new PromoCode
            {
                Id = 1,
                Code = "SUMMER2024",
                Description = "SUMMER2024",
                Percent = 10,
                MaxSaleCents = 5000,
                CreatedDateTime = DateTime.Now
            },
            new PromoCode
            {
                Id = 2,
                Code = "WELCOME10",
                Description = "WELCOME10",
                Percent = 10,
                CreatedDateTime = DateTime.Now
            },
            new PromoCode
            {
                Id = 3,
                Code = "HOLIDAY25",
                Description = "HOLIDAY25",
                Percent = 25,
                MaxSaleCents = 15000,
                CreatedDateTime = DateTime.Now
            },
            new PromoCode
            {
                Id = 4,
                Code = "SPRING2024",
                Description = "SPRING2024",
                Percent = 15,
                MaxSaleCents = 8000,
                CreatedDateTime = DateTime.Now
            });
    }
}
