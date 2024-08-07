using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Egost.Data;
using Egost.Areas.Identity.Data;
using DotNetEnv;


var builder = WebApplication.CreateBuilder(args); 
var config = builder.Configuration;

Env.Load();
builder.Configuration.AddEnvironmentVariables();
builder.Services.AddControllersWithViews();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<EgostContext>(options => options.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<User>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<EgostContext>();

// Add services to the container.
builder.Services.AddControllersWithViews();

builder.Services.AddHttpClient();

builder.Services.AddAuthentication()
   .AddGoogle(options =>
   {
       IConfigurationSection googleAuthNSection =
       config.GetSection("Authentication:Google");
       options.ClientId = googleAuthNSection["ClientId"]!;
       options.ClientSecret = googleAuthNSection["ClientSecret"]!;
   })
   .AddFacebook(options =>
   {
       IConfigurationSection FBAuthNSection =
       config.GetSection("Authentication:Facebook");
       options.AppId = FBAuthNSection["AppId"]!;
       options.AppSecret = FBAuthNSection["AppSecret"]!;
   })
   .AddMicrosoftAccount(microsoftOptions =>
   {
       microsoftOptions.ClientId = config["Authentication:Microsoft:ClientId"]!;
       microsoftOptions.ClientSecret = config["Authentication:Microsoft:ClientSecret"]!;
   });


var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
     var services = scope.ServiceProvider;
    var dbContext = services.GetRequiredService<EgostContext>();
    var userManager = services.GetRequiredService<UserManager<User>>();
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    // Apply migrations
    dbContext.Database.Migrate();

    // Seed roles
    await SeedRolesAsync(roleManager);

    // Seed super user
    await SeedSuperUserAsync(userManager, config);
}

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Store}/{action=Home}");

app.MapRazorPages();

app.Run();


static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager)
{
    string[] roleNames = { "Admin", "Moderator", "Transporter" };
    foreach (var roleName in roleNames)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new IdentityRole(roleName));
        }
    }
}

static async Task SeedSuperUserAsync(UserManager<User> userManager, IConfiguration config)
{
    IConfigurationSection superUserData = config.GetSection("SuperUser");
    var superUser = await userManager.FindByEmailAsync(superUserData["Email"]!);
    if (superUser == null)
    {
        superUser = new User { 
            Name = superUserData["UserName"]!,
            UserName = superUserData["UserName"],
            Email = superUserData["Email"],
            EmailConfirmed = true,
            Gender = "Male"
        };
        var result = await userManager.CreateAsync(superUser, superUserData["Password"]!);
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(superUser, "Admin");
        }
        else
        {
            // Log the errors
            foreach (var error in result.Errors)
            {
                Console.WriteLine($"Error: {error.Description}");
            }
        }
    }
}
