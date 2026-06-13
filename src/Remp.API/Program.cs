using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Remp.Models.Entities;
using Remp.DataAccess.Data;
using Remp.Service.Services;
using Remp.Service.Interfaces;
using Remp.Common.Utilities;

var builder = WebApplication.CreateBuilder(args);

// Register DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// Register ASP.NET Identity
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;           
    options.Password.RequireUppercase = false;      
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();    

builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();
