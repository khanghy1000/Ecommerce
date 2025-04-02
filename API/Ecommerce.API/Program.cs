using System.Text.Json.Serialization;
using Ecommerce.API;
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
using Ecommerce.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
    .Services.AddControllers()
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
    opt.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"));
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

app.Run();
