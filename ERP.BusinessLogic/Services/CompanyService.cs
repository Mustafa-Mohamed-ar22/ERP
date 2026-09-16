using Mapster;
using Microsoft.AspNetCore.Http;

public class CompanyService : ICompanyService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUser;
    private readonly ApplicationDbContext _context;
    public CompanyService(IUnitOfWork unitOfWork, ICurrentUserService currentUser, ApplicationDbContext context)
    {
        _unitOfWork = unitOfWork;
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<Result<CompanyResponse>> GetCurrentAsync(CancellationToken ct = default)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(_currentUser.CompanyId, ct);
        if (company is null)
            return Result.Failure<CompanyResponse>(CompanyErrors.NotFound);

        return Result.Success(company.Adapt<CompanyResponse>());
    }

    public async Task<Result<CompanyResponse>> UpdateCurrentAsync(UpdateCompanyRequest request, CancellationToken ct = default)
    {
        var company = await _unitOfWork.Companies.GetByIdAsync(_currentUser.CompanyId, ct);
        if (company is null)
            return Result.Failure<CompanyResponse>(CompanyErrors.NotFound);
        var existingCompany =  _context.Companies.FirstOrDefault(c => c.Name == request.Name && c.Id!=_currentUser.CompanyId);
        if (existingCompany is not null)
            return Result.Failure<CompanyResponse>
                (new Error("CompanyAlreadyExists", "A company with the same name already exists.", StatusCodes.Status400BadRequest));
        company.Name = request.Name;
        company.LegalName = request.LegalName;
        company.TaxNumber = request.TaxNumber;
        company.Currency = request.Currency;
        company.Country = request.Country;
        company.IsActive = request.IsActive;

        _unitOfWork.Companies.Update(company);
        await _unitOfWork.SaveChangesAsync(ct);

        return Result.Success(company.Adapt<CompanyResponse>());
    }
}