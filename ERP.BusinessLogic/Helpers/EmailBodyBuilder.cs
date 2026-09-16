public static class EmailBodyBuilder
{
    public static string GenerateEmailBody(string contentRootPath, string templateName, Dictionary<string, string> placeholders)
    {
        var templatePath = Path.Combine(contentRootPath, "Templates", $"{templateName}.html");
        var body = File.ReadAllText(templatePath);

        foreach (var (key, value) in placeholders)
            body = body.Replace(key, value);

        return body;
    }
}
