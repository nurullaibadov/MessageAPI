using MessageAPI.API.Middleware;
using MessageAPI.Application;
using MessageAPI.Infrastructure;
using MessageAPI.Infrastructure.Data;
using MessageAPI.Infrastructure.SignalR;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using FluentValidation;

using Serilog;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

#region Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();
#endregion

#region Controllers
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
#endregion

#region FluentValidation (Manual)
builder.Services.AddValidatorsFromAssembly(
    Assembly.GetExecutingAssembly());

// Əgər validatorlar Application layer-dədirsə:
builder.Services.AddValidatorsFromAssembly(
    typeof(ApplicationServiceRegistration).Assembly);
#endregion

#region Swagger
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MessageAPI",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});
#endregion

#region JWT
var jwtSection = builder.Configuration.GetSection("JwtSettings");

builder.Services.AddAuthentication(o =>
{
    o.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    o.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(o =>
{
    o.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSection["Issuer"],
        ValidAudience = jwtSection["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSection["SecretKey"]!)),
        ClockSkew = TimeSpan.Zero
    };
});
#endregion

#region Authorization
builder.Services.AddAuthorization();
#endregion

#region CORS
builder.Services.AddCors(o =>
{
    o.AddPolicy("Development", p =>
        p.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});
#endregion

#region DI Layers
builder.Services.AddApplicationServices();
builder.Services.AddInfrastructureServices(builder.Configuration);
#endregion

var app = builder.Build();

#region Database Migration
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}
#endregion

    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("Development");

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.UseMiddleware<ExceptionMiddleware>();

app.MapControllers();
app.MapHub<ChatHub>("/hubs/chat");

app.Run();