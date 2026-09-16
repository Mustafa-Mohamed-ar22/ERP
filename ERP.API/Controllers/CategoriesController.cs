using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    public CategoriesController(ICategoryService categoryService) => _categoryService = categoryService;

    [HttpGet]
    [Authorize(Policy = "inventory.products.view")]
    public async Task<ActionResult<List<CategoryResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _categoryService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "inventory.products.view")]
    public async Task<ActionResult<CategoryResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _categoryService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "inventory.categories.manage")]
    public async Task<ActionResult<CategoryResponse>> CreateAsync(
        [FromBody] CreateCategoryRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _categoryService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = "inventory.categories.manage")]
    public async Task<ActionResult<CategoryResponse>> UpdateAsync(
        Guid id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _categoryService.UpdateAsync(id, request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "inventory.categories.manage")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _categoryService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}