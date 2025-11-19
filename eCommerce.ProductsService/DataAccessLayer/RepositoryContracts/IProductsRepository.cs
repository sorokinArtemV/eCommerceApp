using System.Linq.Expressions;
using DataAccessLayer.Entities;

namespace DataAccessLayer.RepositoryContracts;

/// <summary>
/// Represents a repo contract for Products table
/// </summary>
public interface IProductsRepository
{
    /// <summary>
    /// Retrieves all products from Products table asynchronously
    /// </summary>
    /// <returns></returns>
    public Task<IEnumerable<Product>> GetProductsAsync();

    /// <summary>
    /// Retrieves all products from Products table by condition asynchronously
    /// </summary>
    /// <param name="expression">The condition to filter</param>
    /// <returns>Returns a collection of matching products</returns>
    public Task<IEnumerable<Product>> GetProductsByConditionAsync(Expression<Func<Product, bool>> expression);

    /// <summary>
    /// Retrieves a product from products table by condition async
    /// </summary>
    /// <param name="expression">The condition to filter</param>
    /// <returns>Returns a single product or null if not found</returns>
    public Task<Product?> GetProductByConditionAsync(Expression<Func<Product, bool>> expression);

    /// <summary>
    /// Adds a new product to database async
    /// </summary>
    /// <param name="product">Product object</param>
    /// <returns>Returns the added product or null in unsuccessful</returns>
    public Task<Product?> AddProductAsync(Product product);

    /// <summary>
    /// Updates an existing product to database async
    /// </summary>
    /// <param name="product">Product object</param>
    /// <returns>Returns the updated product or null in unsuccessful</returns>
    public Task<Product?> UpdateProductAsync(Product product);

    /// <summary>
    /// Deletes an existing product from database async
    /// </summary>
    /// <param name="id">Product id</param>
    /// <returns>Returns true if product is deleted, false otherwise</returns>
    public Task<bool> DeleteProductAsync(Guid id);
}