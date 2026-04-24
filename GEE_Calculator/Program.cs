using GEE_Calculator;
using GEE_Calculator.Application;
using GEE_Calculator.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services
    .AddApplicationServices()
    .AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.UseApiDocumentation();
app.UseHttpsRedirection();
app.UseTenantResolution();
app.UseAuthorization();
app.MapControllers();

app.Run();
