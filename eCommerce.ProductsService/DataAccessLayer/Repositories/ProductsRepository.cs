using System.Linq.Expressions;
using DataAccessLayer.Context;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using Microsoft.EntityFrameworkCore;

namespace DataAccessLayer.Repositories;

public sealed class ProductsRepository : IProductsRepository
{
    private readonly ApplicationDbContext _dbContext;

    public ProductsRepository(ApplicationDbContext context)
    {
        _dbContext = context;
    }

    public async Task<IEnumerable<Product>> GetProductsAsync()
    {
        return await _dbContext.Products.ToListAsync();
    }

    public async Task<IEnumerable<Product>> GetProductsByConditionAsync(Expression<Func<Product, bool>> expression)
    {
        return await _dbContext.Products.Where(expression).ToListAsync();
    }

    public async Task<Product?> GetProductByConditionAsync(Expression<Func<Product, bool>> expression)
    {
        return await _dbContext.Products.FirstOrDefaultAsync(expression);
    }

    public async Task<Product?> AddProductAsync(Product product)
    {
        _dbContext.Products.Add(product);
        await _dbContext.SaveChangesAsync();

        return product;
    }

    public async Task<Product?> UpdateProductAsync(Product product)
    {
        Product? existingProduct = await _dbContext.Products.FirstOrDefaultAsync(x => x.ProductId == product.ProductId);

        if (existingProduct == null) return null;

        existingProduct.ProductName = product.ProductName;
        existingProduct.Category = product.Category;
        existingProduct.QuantityInStock = product.QuantityInStock;
        existingProduct.UnitPrice = product.UnitPrice;

        await _dbContext.SaveChangesAsync();

        return existingProduct;
    }

    public async Task<bool> DeleteProductAsync(Guid id)
    {
        Product? product = await _dbContext.Products.FirstOrDefaultAsync(x => x.ProductId == id);

        if (product == null) return false;

        _dbContext.Products.Remove(product);
        int rowsAffected = await _dbContext.SaveChangesAsync();

        return rowsAffected > 0;
    }
}