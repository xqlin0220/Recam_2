using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Remp.Models.Entities;
using Remp.DataAccess.Data;
using Remp.Service.Services;
using Remp.Service.Interfaces;
using Remp.Common.Utilities;
using Remp.API.Middlewares;
using Remp.API.Extensions;

var builder = WebApplication.CreateBuilder(args);

// 1. DbContext Registration
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")
    )
);

// 2. ASP.NET Identity Registration
builder.Services.AddIdentity<AppUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 8;           
    options.Password.RequireUppercase = false;      
    options.Password.RequireNonAlphanumeric = false;
})
.AddEntityFrameworkStores<AppDbContext>()
.AddDefaultTokenProviders();    

// 3. Custom Services (DI)
builder.Services.AddScoped<IEmailSenderService, EmailSenderService>();

// 4. Controllers
builder.Services.AddControllers();

// 5. Middleware Pipeline
var app = builder.Build();
app.UseExceptionMiddleware();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();