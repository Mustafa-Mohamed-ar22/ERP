using Microsoft.EntityFrameworkCore;
public class ProductService : IProductService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public ProductService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<List<ProductResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var products = await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .OrderBy(p => p.Sku)
            .ToListAsync(ct);

        return Result.Success(products.Select(ToResponse).ToList());
    }

    public async Task<Result<ProductResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.Query()
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

        if (product is null)
            return Result.Failure<ProductResponse>(ProductErrors.NotFound);

        return Result.Success(ToResponse(product));
    }
    public async Task<Result<ProductResponse>> CreateAsync(CreateProductRequest request, CancellationToken ct = default)
    {
        var duplicateExists = await _unitOfWork.Products.Query().AnyAsync(p => p.Sku == request.Sku, ct);
        if (duplicateExists)
            return Result.Failure<ProductResponse>(ProductErrors.DuplicateSku);
        var existingProductName = await _unitOfWork.Products.Query().AnyAsync(p => p.Name == request.Name, ct);
        if (existingProductName)
            return Result.Failure<ProductResponse>(ProductErrors.DuplicateName);

        ProductCategory? category = null;
        if (request.CategoryId is { } categoryId)
        {
            category = await _unitOfWork.ProductCategories.GetByIdAsync(categoryId, ct);
            if (category is null)
                return Result.Failure<ProductResponse>(CategoryErrors.NotFound);
        }

        var product = new Product
        {
            CompanyId = _currentUser.CompanyId,
            Sku = request.Sku,
            Name = request.Name,
            Description = request.Description,
            UnitOfMeasure = request.UnitOfMeasure,
            CategoryId = request.CategoryId,
            CostPrice = request.CostPrice,
            SalePrice = request.SalePrice,
            IsActive = true
        };

        await _unitOfWork.Products.AddAsync(product, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(ToResponse(product, category));
    }

    public async Task<Result<ProductResponse>> UpdateAsync(Guid id, UpdateProductRequest request, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, ct);
        if (product is null)
            return Result.Failure<ProductResponse>(ProductErrors.NotFound);

        var duplicateExists = await _unitOfWork.Products.Query().AnyAsync(p => p.Sku == request.Sku && p.Id != id, ct);
        if (duplicateExists)
            return Result.Failure<ProductResponse>(ProductErrors.DuplicateSku);

        ProductCategory? category = null;
        if (request.CategoryId is { } categoryId)
        {
            category = await _unitOfWork.ProductCategories.GetByIdAsync(categoryId, ct);
            if (category is null)
                return Result.Failure<ProductResponse>(CategoryErrors.NotFound);
        }
        var existingProductName = await _unitOfWork.Products.Query().AnyAsync(p => p.Id != id && p.Name == request.Name, ct);
        if (existingProductName)
            return Result.Failure<ProductResponse>(ProductErrors.DuplicateName);
        product.Sku = request.Sku;
        product.Name = request.Name;
        product.Description = request.Description;
        product.UnitOfMeasure = request.UnitOfMeasure;
        product.CategoryId = request.CategoryId;
        product.CostPrice = request.CostPrice;
        product.SalePrice = request.SalePrice;
        product.IsActive = request.IsActive;

        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(ToResponse(product, category));
    }
    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var product = await _unitOfWork.Products.GetByIdAsync(id, ct);
        if (product is null)
            return Result.Failure(ProductErrors.NotFound);

        var hasStockOrMovements = await _unitOfWork.StockItems.Query().AnyAsync(s => s.ProductId == id && s.QuantityOnHand != 0, ct)
            || await _unitOfWork.StockMovements.Query().AnyAsync(m => m.ProductId == id, ct);

        if (hasStockOrMovements)
            return Result.Failure(ProductErrors.HasStockOrMovements);

        product.IsDeleted = true;
        _unitOfWork.Products.Update(product);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }
    private static ProductResponse ToResponse(Product p) => ToResponse(p, p.Category);

    private static ProductResponse ToResponse(Product p, ProductCategory? category) => new(
        p.Id, p.Sku, p.Name, p.Description, p.UnitOfMeasure, p.CategoryId, category?.Name, p.CostPrice, p.SalePrice, p.IsActive);
}