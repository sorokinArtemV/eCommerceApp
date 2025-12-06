using System.Linq.Expressions;
using AutoMapper;
using BusinessLogicLayer.DTO;
using BusinessLogicLayer.RabbitMQ;
using BusinessLogicLayer.RabbitMQ.Publisher;
using BusinessLogicLayer.ServiceContracts;
using DataAccessLayer.Entities;
using DataAccessLayer.RepositoryContracts;
using FluentValidation;
using FluentValidation.Results;

namespace BusinessLogicLayer.Services;

public sealed class ProductsService : IProductsService
{
    private readonly IMapper _mapper;
    private readonly IProductsRepository _productsRepository;
    private readonly IValidator<ProductAddRequest> _productAddRequestValidator;
    private readonly IValidator<ProductUpdateRequest> _productUpdateRequestValidator;
    private readonly IRabbitMqPublisher _publisher;


    public ProductsService(
        IMapper mapper,
        IProductsRepository productsRepository,
        IValidator<ProductAddRequest> productAddRequestValidator,
        IValidator<ProductUpdateRequest> productUpdateRequestValidator,
        IRabbitMqPublisher publisher)
    {
        _mapper = mapper;
        _productsRepository = productsRepository;
        _productAddRequestValidator = productAddRequestValidator;
        _productUpdateRequestValidator = productUpdateRequestValidator;
        _publisher = publisher;
    }

    public async Task<List<ProductResponse?>> GetProductsAsync()
    {
        IEnumerable<Product> products = await _productsRepository.GetProductsAsync();

        IEnumerable<ProductResponse?> productResponses = _mapper.Map<IEnumerable<ProductResponse>>(products);

        return productResponses.ToList();
    }

    public async Task<List<ProductResponse?>> GetProductsByConditionAsync(Expression<Func<Product, bool>> expression)
    {
        IEnumerable<Product> products = await _productsRepository.GetProductsByConditionAsync(expression);

        IEnumerable<ProductResponse?> productResponses = _mapper.Map<IEnumerable<ProductResponse>>(products);

        return productResponses.ToList();
    }

    public async Task<ProductResponse?> GetProductByConditionAsync(Expression<Func<Product, bool>> expression)
    {
        Product? product = await _productsRepository.GetProductByConditionAsync(expression);

        return product == null ? null : _mapper.Map<ProductResponse>(product);
    }

    public async Task<ProductResponse?> AddProductAsync(ProductAddRequest? productAddRequest)
    {
        ArgumentNullException.ThrowIfNull(productAddRequest);

        ValidationResult validationResult = await _productAddRequestValidator.ValidateAsync(productAddRequest);

        if (!validationResult.IsValid)
        {
            string errorMessages = string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage));
            throw new ValidationException(errorMessages);
        }

        Product product = _mapper.Map<Product>(productAddRequest);
        Product? addedProduct = await _productsRepository.AddProductAsync(product);

        return addedProduct == null ? null : _mapper.Map<ProductResponse>(addedProduct);
    }

    public async Task<ProductResponse?> UpdateProductAsync(ProductUpdateRequest productUpdateRequest)
    {
        Product? product = await _productsRepository
            .GetProductByConditionAsync(x => x.ProductId == productUpdateRequest.ProductId);

        ArgumentNullException.ThrowIfNull(product);

        ValidationResult validationResult = await _productUpdateRequestValidator.ValidateAsync(productUpdateRequest);

        if (!validationResult.IsValid)
        {
            string errorMessages = string.Join(", ", validationResult.Errors.Select(x => x.ErrorMessage));
            throw new ValidationException(errorMessages);
        }

        Product productToUpdate = _mapper.Map<Product>(productUpdateRequest);

        bool isProductNameChanged = product.ProductName != productUpdateRequest.ProductName;

        if (isProductNameChanged)
        {
            const string RoutingKey = "product.updated.name";

            ProductNameUpdateMessage message = new ProductNameUpdateMessage(
               ProductId: productUpdateRequest.ProductId,
               NewProductName: productUpdateRequest.ProductName,
               PublishedAt: DateTimeOffset.UtcNow
            );

            await _publisher.PublishAsync(RoutingKey, message);
        }

        Product? updatedProduct = await _productsRepository.UpdateProductAsync(productToUpdate);

        return updatedProduct == null ? null : _mapper.Map<ProductResponse>(updatedProduct);
    }

    public async Task<bool> DeleteProductAsync(Guid productId)
    {
        Product? product = await _productsRepository.GetProductByConditionAsync(x => x.ProductId == productId);

        if (product is null) return false;

        bool isDeleted = await _productsRepository.DeleteProductAsync(productId);

        return isDeleted;
    }
}