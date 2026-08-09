using FeedbackService.Data;
using FeedbackService.Repositories;
using FeedbackService.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using FeedbackService.Eureka;

var builder = WebApplication.CreateBuilder(args);

// Add Controllers
builder.Services.AddControllers();

builder.Services.Configure<EurekaOptions>(
    builder.Configuration.GetSection("Eureka"));

// Configure MySQL Database
builder.Services.AddDbContext<AppDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    options.UseMySql(
        connectionString!,
        ServerVersion.AutoDetect(connectionString)
    );
});


builder.Services.AddScoped<IFeedbackRepository, FeedbackRepository>();

builder.Services.AddScoped<
    FeedbackService.Services.IFeedbackService,
    FeedbackService.Services.FeedbackService>();

builder.Services.AddHostedService<EurekaRegistrationService>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,

            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)
            ),


            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };

        options.MapInboundClaims = false;

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine("JWT Authentication Failed:");
                Console.WriteLine(context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var identity = (ClaimsIdentity)context.Principal.Identity!;

                var sub = identity.FindFirst("sub");
                if (sub != null)
                {
                    identity.AddClaim(
                        new Claim(ClaimTypes.NameIdentifier, sub.Value));
                }

                var role = identity.FindFirst("role");
                if (role != null)
                {
                    identity.AddClaim(
                        new Claim(ClaimTypes.Role, role.Value));
                }

                return Task.CompletedTask;
            }

        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();