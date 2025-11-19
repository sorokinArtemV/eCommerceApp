using DataAccessLayer.Context;
using DataAccessLayer.Repositories;
using DataAccessLayer.RepositoryContracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DataAccessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Add Data Access Layer services into the IoC container

        services.AddDbContext<ApplicationDbContext>(options => options
            .UseMySQL(configuration.GetConnectionString("MySqlConnection")!));

        services.AddScoped<IProductsRepository, ProductsRepository>();

        return services;
    }
}