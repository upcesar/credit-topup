using CreditBoost.Infra.Auth.Models;
using CreditBoost.Infra.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace CreditBoost.Api.Configurations;

public static class AuthConfigurations
{
    public static void AddAppAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddIdentityServerAuthentication();
        services.AddJwtAuthentication(configuration);

        services.AddHttpContextAccessor();
        services.AddScoped<IAuthenticatedUser, AuthenticatedUser>();
    }

    private static void AddIdentityServerAuthentication(this IServiceCollection services)
    {
        // Add IdentityServer
        services.AddIdentityCore<IdentityUser>()
                .AddEntityFrameworkStores<CreditBoostDbContext>()
                .AddDefaultTokenProviders();
    }

    private static void AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSection = configuration.GetSection("JWT");

        services.Configure<JwtOptions>(jwtSection);
        var jwt = jwtSection.Get<JwtOptions>();

        // Adding Authentication
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = jwt.ValidAudience,
                ValidIssuer = jwt.ValidIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret))
            };
        });
    }
}

public class JwtOptions
{
    public string ValidAudience { get; set; }
    public string ValidIssuer { get; set; }
    public string Secret { get; set; }
}
