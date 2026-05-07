using GEE_Calculator.Application;
using GEE_Calculator.Application.Auth;
using GEE_Calculator.Infrastructure;
using GEE_Calculator.Integration;
using GEE_Calculator.WebApi;
using GEE_Calculator.WebApi.Auth;
using GEE_Calculator.WebApi.Tenancy;
using GEE_Calculator.Domain.Auth;
using GEE_Calculator.Domain.Tenancy;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentTenantAccessor, CurrentTenantAccessor>();
builder.Services.AddScoped<ICurrentUserContext, CurrentUserContext>();
builder.Services.AddScoped<IAccessTokenReader, HttpAccessTokenReader>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var keycloakSection = builder.Configuration.GetSection("Authentication:Keycloak");
        var authority = keycloakSection["Authority"];
        var audience = keycloakSection["Audience"];

        if (!string.IsNullOrWhiteSpace(authority))
        {
            options.Authority = authority;
        }

        if (!string.IsNullOrWhiteSpace(audience))
        {
            options.Audience = audience;
        }

        options.RequireHttpsMetadata = keycloakSection.GetValue("RequireHttpsMetadata", true);
        options.TokenValidationParameters.ValidateAudience = !string.IsNullOrWhiteSpace(audience);
    });
builder.Services.AddAuthorization();
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration)
    .AddIntegrationServices(builder.Configuration);

var app = builder.Build();

await app.InitializeDatabaseAsync();
app.UseApiDocumentation();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseTenantResolution();
app.UseAuthorization();
app.MapControllers();

app.Run();
