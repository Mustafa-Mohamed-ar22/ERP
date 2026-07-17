using Microsoft.AspNetCore.Mvc;
using System.Globalization;

public static class ResultExtensions
{
    public static ObjectResult ToProblem(this Result result)
    {
        if (result.IsSuccess)
            throw new InvalidOperationException("Can't convert a success result to a problem");

        var problem = Results.Problem(statusCode: result.Error.StatusCode);
        var problemDetails = problem.GetType().GetProperty(nameof(ProblemDetails))!
            .GetValue(problem) as ProblemDetails;

        var isArabic = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName
            .Equals("ar", StringComparison.OrdinalIgnoreCase);

        var message = isArabic ? result.Error.ErrorDescriptionAr : result.Error.ErrorDescription;

        problemDetails!.Extensions = new Dictionary<string, object?>
        {
            { "errors", new[] { result.Error.code, message } }
        };

        return new ObjectResult(problemDetails) { StatusCode = result.Error.StatusCode };
    }
}