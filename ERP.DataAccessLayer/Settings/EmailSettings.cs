public class EmailSettings
{
    public string Host { get; set; } = default!;
    public int Port { get; set; }
    public string SenderEmail { get; set; } = default!;
    public string Password { get; set; } = default!;
}
