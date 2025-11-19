using System.Linq.Expressions;
using BusinessLogicLayer.DTO;
using DataAccessLayer.Entities;

namespace BusinessLogicLayer.ServiceContracts;

/// <summary>
/// Represents contract for Products service
/// </summary>
public interface IProductsService
{
    /// <summary>
    /// Retrieves the list of products from products repo async
    /// </summary>
    /// <returns>Returns list of <see cref="ProductResponse"/> or null.</returns>
    public Task<List<ProductResponse?>> GetProductsAsync();

    /// <summary>
    /// Retrieves list of products by condition async
    /// </summary>
    /// <param name="expression">Condition to check</param>
    /// <returns>Returns list of <see cref="ProductResponse"/> or null.</returns>
    public Task<List<ProductResponse?>> GetProductsByConditionAsync(Expression<Func<Product, bool>> expression);

    /// <summary>
    /// Retrieves a single product by condition async
    /// </summary>
    /// <param name="expression"></param>
    /// <returns>Returns matching product or null</returns>
    public Task<ProductResponse?> GetProductByConditionAsync(Expression<Func<Product, bool>> expression);

    /// <summary>
    /// Adds product to products repo
    /// </summary>
    /// <param name="productAddRequest"><see cref="ProductAddRequest"/></param>
    /// <returns>Returns ProductResponse or null</returns>
    public Task<ProductResponse?> AddProductAsync(ProductAddRequest? productAddRequest);

    /// <summary>
    /// Updates the existing product
    /// </summary>
    /// <param name="productAddRequest"><see cref="ProductUpdateRequest"/></param>
    /// <returns>Returns updated product or null</returns>
    public Task<ProductResponse?> UpdateProductAsync(ProductUpdateRequest productAddRequest);

    /// <summary>
    /// Deletes and existing product
    /// </summary>
    /// <param name="productId">Product ID</param>
    /// <returns>Returns true if deleted, otherwise false</returns>
    public Task<bool> DeleteProductAsync(Guid productId);
}