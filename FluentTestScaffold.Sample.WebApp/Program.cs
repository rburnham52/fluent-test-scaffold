using System.Net;
using FluentTestScaffold.Sample.Data;
using FluentTestScaffold.Sample.Services;
using FluentTestScaffold.Sample.WebApp.Exceptions.Filters;
using FluentTestScaffold.Sample.WebApp.Services;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ShoppingCartService>();
builder.Services.AddSqlServer<TestDbContext>(
    builder.Configuration.GetConnectionString("Sample"),
    options => options.MigrationsAssembly(typeof(ShoppingCart).Assembly.FullName));

builder.Services.AddControllers(
    options => options.Filters.Add<InvalidOperationsExceptionFilter>());

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.ExpireTimeSpan = TimeSpan.FromMinutes(20);
        options.SlidingExpiration = true;

        options.Events.OnRedirectToLogin = (context) => Task.Run(() => context.Response.StatusCode = (int)HttpStatusCode.Unauthorized);
    });
builder.Services.AddScoped<IUserRequestContext, HandRolledUserRequestContext>();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

/// <summary>
/// Required when used with a WebApplicationFactory for testing
/// </summary>
public partial class Program {}