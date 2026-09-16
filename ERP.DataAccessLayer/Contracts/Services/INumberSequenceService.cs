public interface INumberSequenceService
{
    Task<string> GetNextNumberAsync(string documentType, string prefix, int padding, CancellationToken ct = default);
}