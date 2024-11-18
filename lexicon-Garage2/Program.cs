using lexicon_Garage2.Data;
using lexicon_Garage2.Extensions;
using lexicon_Garage2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Configure DbContext
builder.Services.AddDbContext<lexicon_Garage2Context>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("lexicon_Garage2Context")
            ?? throw new InvalidOperationException(
                "Connection string 'lexicon_Garage2Context' not found."
            )
    )
);

// Configure Identity
builder
    .Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<lexicon_Garage2Context>();

// Add services to the container
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure middleware
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

await app.SeedDataAsync();

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication(); // Ensure this is included for Identity
app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Vehicles}/{action=Garage}/{id?}");
app.MapRazorPages();

app.Run();
