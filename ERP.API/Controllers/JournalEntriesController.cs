using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class JournalEntriesController : ControllerBase
{
    private readonly IJournalEntryService _journalEntryService;
    public JournalEntriesController(IJournalEntryService journalEntryService) => _journalEntryService = journalEntryService;

    [HttpGet]
    [Authorize(Policy = "accounting.journal.view")]
    public async Task<ActionResult<List<JournalEntryResponse>>> GetAllAsync(CancellationToken cancellationToken = default!)
    {
        var result = await _journalEntryService.GetAllAsync(cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = "accounting.journal.view")]
    public async Task<ActionResult<JournalEntryResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _journalEntryService.GetByIdAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost]
    [Authorize(Policy = "accounting.journal.create")]
    public async Task<ActionResult<JournalEntryResponse>> CreateAsync(
        [FromBody] CreateJournalEntryRequest request, CancellationToken cancellationToken = default!)
    {
        var result = await _journalEntryService.CreateAsync(request, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("{id:guid}/post")]
    [Authorize(Policy = "accounting.journal.post")]
    public async Task<ActionResult<JournalEntryResponse>> PostAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _journalEntryService.PostAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpPost("{id:guid}/reverse")]
    [Authorize(Policy = "accounting.journal.reverse")]
    public async Task<ActionResult<JournalEntryResponse>> ReverseAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _journalEntryService.ReverseAsync(id, cancellationToken);
        return result.IsSuccess ? Ok(result.Data) : result.ToProblem();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "accounting.journal.create")]
    public async Task<ActionResult> DeleteAsync(Guid id, CancellationToken cancellationToken = default!)
    {
        var result = await _journalEntryService.DeleteAsync(id, cancellationToken);
        return result.IsSuccess ? Ok() : result.ToProblem();
    }
}