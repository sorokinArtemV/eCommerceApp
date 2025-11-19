using System.Text.Json.Serialization;
using BusinessLogicLayer;
using DataAccessLayer;
using FluentValidation.AspNetCore;
using ProductsService.API.ApiEndpoints;
using ProductsService.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add DAL and BLL services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer();

builder.Services.AddControllers();

builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cors
builder.Services.AddCors(options =>
{
options.AddDefaultPolicy(b => b.WithOrigins("http://localhost:4200").AllowAnyMethod().AllowAnyHeader());
});

builder.Services.AddFluentValidationAutoValidation();

var app = builder.Build();

app.UseExceptionHandlingMiddleware();
app.UseRouting();


// Cors
app.UseCors();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Auth
app.UseHttpsRedirection();
app.UseAuthorization();
app.UseAuthorization();

app.MapControllers();

app.MapProductApiEndpoints();

app.Run();