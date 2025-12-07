using BusinessLogicLayer;
using BusinessLogicLayer.HttpClients;
using BusinessLogicLayer.Policies;
using BusinessLogicLayer.RabbitMQ;
using BusinessLogicLayer.RabbitMQ.ConnectionService;
using BusinessLogicLayer.RabbitMQ.RabbitMQOptions;
using DataAccessLayer;
using FluentValidation.AspNetCore;
using OrdersService.API.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Add DAL and BLL services
builder.Services.AddDataAccessLayer(builder.Configuration);
builder.Services.AddBusinessLogicLayer(builder.Configuration);

builder.Services.AddControllers();

builder.Services.AddAutoMapper(_ => { }, typeof(Program));
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddSingleton<UsersMicroservicePolicies>();
builder.Services.AddSingleton<ProductMicroservicePolicies>();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Cors
builder.Services.AddCors(options => options.AddDefaultPolicy(b =>
{
    b.WithOrigins("http://localhost:4200").AllowAnyHeader().AllowAnyMethod();
}));

builder.Services.Configure<UsersPoliciesSettings>(builder.Configuration.GetSection("UsersPolicies"));

builder.Services.AddHttpClient<UsersMicroServiceClient>(client =>
{
    client.BaseAddress = new Uri($"http://" +
                                 $"{builder.Configuration["UsersMicroserviceName"]}:" +
                                 $"{builder.Configuration["UsersMicroservicePort"]}");
}).AddPolicyHandler((sp, _) =>
{
    UsersMicroservicePolicies policies = sp.GetRequiredService<UsersMicroservicePolicies>();
    return policies.AllPolicies;
});

// RabbitMQ
// 1. Опции
builder.Services.Configure<RabbitMqOptions>(
    builder.Configuration.GetSection("RabbitMq"));

builder.Services.Configure<RabbitMqConsumerOptions>(
    builder.Configuration.GetSection("RabbitMqConsumer"));

// 2. Connection service (singleton + hosted)
builder.Services.AddSingleton<RabbitMqConnectionService>();

builder.Services.AddSingleton<IRabbitMqConnectionAccessor>(sp =>
    sp.GetRequiredService<RabbitMqConnectionService>());

builder.Services.AddHostedService(sp =>
    sp.GetRequiredService<RabbitMqConnectionService>());

// 3. Consumer Orders
builder.Services.AddHostedService<RabbitMqProductEventsConsumer>();


builder.Services.AddHttpClient<ProductsMicroserviceClient>(client =>
{
    client.BaseAddress = new Uri($"http://" +
                                 $"{builder.Configuration["ProductsMicroserviceName"]}:" +
                                 $"{builder.Configuration["ProductsMicroservicePort"]}");
}).AddPolicyHandler((sp, _) =>
{
    ProductMicroservicePolicies policies = sp.GetRequiredService<ProductMicroservicePolicies>();
    return policies.AllPolicies;
});

var app = builder.Build();

app.UseExceptionHandlerMiddleware();
app.UseRouting();

// Cors
app.UseCors();

// Swagger
app.UseSwagger();
app.UseSwaggerUI();

// Auth
//app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Endpoints
app.MapControllers();

app.Run();