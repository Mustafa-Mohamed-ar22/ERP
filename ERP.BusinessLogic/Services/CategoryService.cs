using Microsoft.EntityFrameworkCore;

public class CategoryService : ICategoryService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;

    public CategoryService(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
    }

    public async Task<Result<List<CategoryResponse>>> GetAllAsync(CancellationToken ct = default)
    {
        var categories = await _unitOfWork.ProductCategories.Query().ToListAsync(ct);
        return Result.Success(categories.Select(ToResponse).ToList());
    }

    public async Task<Result<CategoryResponse>> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _unitOfWork.ProductCategories.GetByIdAsync(id, ct);
        if (category is null)
            return Result.Failure<CategoryResponse>(CategoryErrors.NotFound);

        return Result.Success(ToResponse(category));
    }

    public async Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken ct = default)
    {
        if (request.ParentCategoryId is { } parentId)
        {
            var parentExists = await _unitOfWork.ProductCategories.Query().AnyAsync(c => c.Id == parentId, ct);
            if (!parentExists)
                return Result.Failure<CategoryResponse>(CategoryErrors.ParentNotFound);
        }
        var existingCategoryName = await _unitOfWork.ProductCategories.Query()
            .AnyAsync(c => c.Name == request.Name, ct);
        if (existingCategoryName)
            return Result.Failure<CategoryResponse>(CategoryErrors.DuplicateName);
        var category = new ProductCategory
        {
            CompanyId = _currentUser.CompanyId,
            Name = request.Name,
            ParentCategoryId = request.ParentCategoryId,
            IsActive = true
        };

        await _unitOfWork.ProductCategories.AddAsync(category, ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(ToResponse(category));
    }

    public async Task<Result<CategoryResponse>> UpdateAsync(Guid id, UpdateCategoryRequest request, CancellationToken ct = default)
    {
        var category = await _unitOfWork.ProductCategories.GetByIdAsync(id, ct);
        if (category is null)
            return Result.Failure<CategoryResponse>(CategoryErrors.NotFound);

        if (request.ParentCategoryId is { } parentId)
        {
            if (parentId == id)
                return Result.Failure<CategoryResponse>(CategoryErrors.InvalidParent);

            var parentExists = await _unitOfWork.ProductCategories.Query().AnyAsync(c => c.Id == parentId, ct);
            if (!parentExists)
                return Result.Failure<CategoryResponse>(CategoryErrors.ParentNotFound);

            var currentAncestorId = (Guid?)parentId;
            var depth = 0;
            while (currentAncestorId is not null && depth < 50)
            {
                if (currentAncestorId == id)
                    return Result.Failure<CategoryResponse>(CategoryErrors.InvalidParent);

                currentAncestorId = await _unitOfWork.ProductCategories.Query()
                    .Where(c => c.Id == currentAncestorId)
                    .Select(c => c.ParentCategoryId)
                    .FirstOrDefaultAsync(ct);
                depth++;
            }
        }
        var existingCategoryName = await _unitOfWork.ProductCategories.Query()
           .AnyAsync(c => c.Id!=id&& c.Name == request.Name, ct);
        if (existingCategoryName)
            return Result.Failure<CategoryResponse>(CategoryErrors.DuplicateName);
        category.Name = request.Name;
        category.ParentCategoryId = request.ParentCategoryId;
        category.IsActive = request.IsActive;

        _unitOfWork.ProductCategories.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(ToResponse(category));
    }

    public async Task<Result> DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var category = await _unitOfWork.ProductCategories.GetByIdAsync(id, ct);
        if (category is null)
            return Result.Failure(CategoryErrors.NotFound);

        var hasChildren = await _unitOfWork.ProductCategories.Query().AnyAsync(c => c.ParentCategoryId == id, ct);
        var hasProducts = await _unitOfWork.Products.Query().AnyAsync(p => p.CategoryId == id, ct);

        if (hasChildren || hasProducts)
            return Result.Failure(CategoryErrors.HasChildrenOrProducts);

        category.IsDeleted = true;
        _unitOfWork.ProductCategories.Update(category);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success();
    }

    private static CategoryResponse ToResponse(ProductCategory c) => new(c.Id, c.Name, c.ParentCategoryId, c.IsActive);
}