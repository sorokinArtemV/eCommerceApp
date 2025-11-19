using System.Text.Json.Serialization;
using eCommerce.API.Middleware;
using eCommerce.Core;
using eCommerce.Core.Mappers;
using eCommerce.Infrastructure;
using FluentValidation.AspNetCore;


var builder = WebApplication.CreateBuilder(args);

// Json log
// builder.Logging.AddJsonConsole();

// Add Infrastructure services
builder.Services.AddInfrastructure();
builder.Services.AddCore();

// Add controllers to the service collection
builder.Services
    .AddControllers()
    .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddAutoMapper(_ => { }, typeof(ApplicationUserMappingProfile));

builder.Services.AddFluentValidationAutoValidation();

// Add API explorer services
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add cors services
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(b => b
        .WithOrigins("http://localhost:4200")
        .AllowAnyMethod().AllowAnyHeader().AllowAnyHeader());
});

var app = builder.Build();

app.UseExceptionHandlerMiddleware();

// Routing
app.UseRouting();

app.UseSwagger();
app.UseSwaggerUI();

// CORS
app.UseCors();

// Auth
app.UseAuthentication();
app.UseAuthorization();

// Controller routes
app.MapControllers();

app.Run();