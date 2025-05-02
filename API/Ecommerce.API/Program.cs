using System.Text.Json.Serialization;
using Ecommerce.API.Extentions;
using Ecommerce.API.Middleware;
using Ecommerce.Application.Core;
using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Products.Queries;
using Ecommerce.Application.Products.Validators;
using Ecommerce.Domain;
using Ecommerce.Infrastructure.Payments;
using Ecommerce.Infrastructure.Photos;
using Ecommerce.Infrastructure.Security;
using Ecommerce.Infrastructure.Shipping;
using Ecommerce.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Scalar.AspNetCore;
using VNPAY.NET;

var builder = WebApplication.CreateBuilder(args);

if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddJsonFile("secrets.json", optional: true, reloadOnChange: true);
}

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder
    .Services.AddControllers(opt =>
    {
        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        opt.Filters.Add(new AuthorizeFilter(policy));
    })
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.Configure<JsonOptions>(opt =>
{
    opt.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddOpenApi();
builder.Services.AddCors();

builder.Services.AddDbContext<AppDbContext>(opt =>
{
    opt.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        o =>
        {
            o.MapEnum<SalesOrderStatus>("sales_order_status");
            o.MapEnum<PaymentMethod>("payment_method");
            o.MapEnum<CouponType>("coupon_type");
            o.MapEnum<CouponDiscountType>("coupon_discount_type");
        }
    );
});

builder.Services.AddScoped<IPhotoService, PhotoService>();
builder.Services.Configure<S3Settings>(builder.Configuration.GetSection("S3Settings"));
builder.Services.AddMediatR(opt =>
{
    opt.RegisterServicesFromAssemblyContaining<ListProducts.Handler>();
    opt.AddOpenBehavior(typeof(ValidationBehavior<,>));
});
builder.Services.AddValidatorsFromAssemblyContaining<CreateProductValidator>();
builder.Services.AddAutoMapper(typeof(MappingProfiles).Assembly);
builder.Services.AddScoped<IUserAccessor, UserAccessor>();
builder.Services.AddTransient<ExceptionMiddleware>();

builder
    .Services.AddIdentityApiEndpoints<User>(opt =>
    {
        opt.User.RequireUniqueEmail = true;
        // opt.SignIn.RequireConfirmedEmail = true;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy(
        "IsProductOwner",
        policy =>
        {
            policy.Requirements.Add(new IsProductOwnerRequirement());
        }
    );

    opt.AddPolicy(
        "HasOrder",
        policy =>
        {
            policy.Requirements.Add(new HasOrderRequirement());
        }
    );

    opt.AddPolicy(
        "IsAddressOwner",
        policy =>
        {
            policy.Requirements.Add(new IsAddressOwnerRequirement());
        }
    );
});

builder.Services.AddTransient<IAuthorizationHandler, IsProductOwnerRequirementHandler>();
builder.Services.AddTransient<IAuthorizationHandler, IsAddressOwnerRequirementHandler>();

builder.Services.ConfigureApplicationCookie(opt =>
{
    opt.Cookie.HttpOnly = true;
    opt.Cookie.SameSite = SameSiteMode.None;
    opt.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    opt.ExpireTimeSpan = TimeSpan.FromDays(7);
});

builder.Services.AddSingleton<IVnpay, Vnpay>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var vnpay = new Vnpay();
    vnpay.Initialize(
        configuration["Vnpay:TmnCode"]!,
        configuration["Vnpay:HashSecret"]!,
        configuration["Vnpay:BaseUrl"]!,
        configuration["Vnpay:CallbackUrl"]!
    );
    return vnpay;
});

builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.Configure<GHNSettings>(builder.Configuration.GetSection("GHN"));
builder.Services.AddSingleton<IShippingService, ShippingService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

app.UseHttpsRedirection();

app.UseMiddleware<ExceptionMiddleware>();

app.UseCors(x =>
    x.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins("http://localhost:5173")
);

app.UseFileServer();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapGroup("api").WithTags("Identity").MapCustomIdentityApi<User>();

using var scope = app.Services.CreateScope();
var services = scope.ServiceProvider;

try
{
    var context = services.GetRequiredService<AppDbContext>();
    var ghnSettings = services.GetRequiredService<IOptions<GHNSettings>>();
    await context.Database.MigrateAsync();

    await Seed.SeedLocations(context, ghnSettings.Value.Token);
}
catch (Exception ex)
{
    var logger = services.GetRequiredService<ILogger<Program>>();
    logger.LogError(ex, "An error occurred during migration.");
}

app.Run();
