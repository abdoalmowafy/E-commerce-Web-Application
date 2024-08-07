using Egost.Areas.Identity.Data;
using Egost.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
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
    public DbSet<ReturnOrder> ReturnOrders { get; set; }
    public DbSet<ReturnProduct> ReturnProducts { get; set; }
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

        var DeletedUserEntries = UserEntries.Where(e => e.State == EntityState.Deleted);
        foreach (var entry in DeletedUserEntries)
        {
            var user = entry.Entity;
            var UserOrders = Orders.Where(o => o.User == user);
            foreach (var order in UserOrders)
            {
                if ( order.DeletedDateTime == null && order.DeliveryDateTime == null)
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
            foreach (var edit in UserEdits)
            {
                EditHistories.Remove(edit);
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Seed Categories
        builder.Entity<Category>().HasData([
            new() { Id = 1, Name = "Sports, Instruments & Accessories" },
            new() { Id = 2, Name = "Toys, Games, Video Games & Accessories" },
            new() { Id = 3, Name = "Arts, Crafts & Sewing" },
            new() { Id = 4, Name = "Clothing, Shoes & Jewelry" },
            new() { Id = 5, Name = "Beauty & Personal Care" },
            new() { Id = 6, Name = "Books" },
            new() { Id = 7, Name = "Electronics & Accessories" },
            new() { Id = 8, Name = "Software" },
            new() { Id = 9, Name = "Grocery & Gourmet Food" },
            new() { Id = 10, Name = "Home Furniture & Accessories" },
            new() { Id = 11, Name = "Luggage & Travel Gear" },
            new() { Id = 12, Name = "Pet Supplies" }
        ]);

        // Seed Products
        builder.Entity<Product>().HasData(new Product()
        {
            Id = 1,
            Name = "Basket Ball",
            Description = "Basket Ball\nBasket Ball\nBasket Ball",
            CategoryId = 1,
            SKU = 50,
            PriceCents = 50000,
            SalePercent = 0,
            CreatedDateTime = DateTime.Now,
        },
        new Product()
        {
            Id = 2,
            Name = "Basket Ball Sale 10%",
            Description = "Basket Ball Sale 10%\nBasket Ball Sale 10%\nBasket Ball Sale 10%",
            CategoryId = 1,
            SKU = 50,
            PriceCents = 50000,
            SalePercent = 10,
            CreatedDateTime = DateTime.Now,
        });

        // Seed Base Address
        builder.Entity<Address>().HasData(Address.Base);

        // Many Users has many wishlist of Products
        builder.Entity<User>()
            .HasMany(u => u.WishList)
            .WithMany(p => p.WishlistUsers);

        // One User has many Orders
        builder.Entity<Order>()
            .HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Restrict);

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

        // One Cart has many CartProducts
        builder.Entity<Cart>()
            .HasMany(c => c.CartProducts)
            .WithOne();

        // One User has many Addresses
        builder.Entity<User>()
            .HasMany(u => u.Addresses)
            .WithOne();

        //// One User has many EditHistories
        //builder.Entity<User>()
        //    .HasMany(u => u.EditsHistory)
        //    .WithOne();

        //// One Review has many EditHistories
        //builder.Entity<Review>()
        //    .HasMany(r => r.EditsHistory)
        //    .WithOne();

        // One Category has many Products
        builder.Entity<Product>()
            .HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId);

        // One Product has many Reviews
        builder.Entity<Product>()
            .HasMany(p => p.Reviews)
            .WithOne();

        // Many Order has one Address
        builder.Entity<Order>()
            .HasOne(o => o.Address)
            .WithMany();

        // Many Order has one PromoCode
        builder.Entity<Order>()
            .HasOne(o => o.PromoCode)
            .WithMany();

        // One Address has many EditHistories
        builder.Entity<Address>()
            .HasMany(a => a.EditsHistory)
            .WithOne();

        // One User has many Searches
        builder.Entity<Search>()
            .HasOne(s => s.User)
            .WithMany();
    }
}
