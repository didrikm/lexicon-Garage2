using lexicon_Garage2.Data;
using lexicon_Garage2.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<lexicon_Garage2Context>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("lexicon_Garage2Context")
            ?? throw new InvalidOperationException(
                "Connection string 'lexicon_Garage2Context' not found."
            )
    )
);

builder
    .Services.AddDefaultIdentity<ApplicationUser>(options =>
        options.SignIn.RequireConfirmedAccount = true
    )
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<lexicon_Garage2Context>();

// Add services to the container.
builder.Services.AddControllersWithViews();

var app = builder.Build();

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

app.UseAuthorization();

app.MapControllerRoute(name: "default", pattern: "{controller=Vehicles}/{action=Garage}/{id?}");
app.MapRazorPages();

app.Run();
