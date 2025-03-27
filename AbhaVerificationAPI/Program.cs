using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using AbhaIntegration.Services;
using Newtonsoft.Json;

var builder = WebApplication.CreateBuilder(args);

// ✅ Add Controllers and NewtonsoftJson
builder.Services.AddControllers()
    .AddNewtonsoftJson();

// ✅ Register Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ✅ Register HttpClientFactory and VerifyService
builder.Services.AddHttpClient<VerifyService>();
builder.Services.AddScoped<VerifyService>();

// ✅ Allow CORS for frontend communication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// ✅ Enable Swagger UI
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "ABHA Verification API v1"));
}

app.Run();
