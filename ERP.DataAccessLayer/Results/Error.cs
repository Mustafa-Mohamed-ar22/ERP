using Microsoft.AspNetCore.Http;

public sealed class Error
{
    public string code { get; }
    public string ErrorDescription { get; }
    public string ErrorDescriptionAr { get; }
    public int StatusCode { get; }

    public Error(string code, string errorDescription, string errorDescriptionAr, int statusCode)
    {
        this.code = code;
        ErrorDescription = errorDescription;
        ErrorDescriptionAr = errorDescriptionAr;
        StatusCode = statusCode;
    }

    public static readonly Error None = new(string.Empty, string.Empty, string.Empty, StatusCodes.Status200OK);
}