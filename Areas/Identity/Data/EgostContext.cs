using Egost.Areas.Identity.Data;
using Egost.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Egost.Data;

public class EgostContext(DbContextOptions<EgostContext> options) : IdentityDbContext<User>(options)
{
    public DbSet<EditHistory> EditHistories { get; set; }
    public DbSet<Address> Addresses { get; set; }
    public DbSet<Product> Products { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartProduct> CartProducts { get; set; }
    public DbSet<PromoCode> PromoCodes { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderProduct> OrderProducts { get; set; }
    public DbSet<ReturnOrder> ReturnOrders { get; set; }
    public DbSet<ReturnProduct> ReturnProducts { get; set; }



    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Add new cart for new users
        var UserEntries = ChangeTracker.Entries<User>();

        var AddedUserEntries = UserEntries.Where(e => e.State == EntityState.Added);
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
        //var entries = ChangeTracker.Entries().Where(e => e.State == EntityState.Modified);
        //foreach (var entry in entries)
        //{
        //    var OriginalValues = entry.OriginalValues;
        //    var CurrentValues = entry.CurrentValues;
        //    foreach (var property in OriginalValues.Properties)
        //    {

        //        if (!object.Equals(OriginalValues[property], CurrentValues[property]))
        //        {
        //            EditHistory Edit = new()
        //            {
        //                Editor = user,
        //                Field = property.Name,
        //                OldData = OriginalValues[property].ToString(),
        //                NewData = CurrentValues[property].ToString(),
        //            };
        //            EditHistories.Add(Edit);
        //            object obj = entry.Entity;
                    
        //            user.EditsHistory.Add(Edit);
        //        }
        //    }
        //}
        base.OnModelCreating(builder);
    }
}
