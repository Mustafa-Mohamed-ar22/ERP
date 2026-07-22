public interface ICompanyService
{
    Task<Result<CompanyResponse>> GetCurrentAsync(CancellationToken ct = default);
    Task<Result<CompanyResponse>> UpdateCurrentAsync(UpdateCompanyRequest request, CancellationToken ct = default);
}
