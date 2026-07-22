// DTO's/Auth/LoginRequest.cs
// DTO's/Auth/RegisterRequest.cs
// DTO's/Auth/ConfirmEmailRequest.cs
// DTO's/Auth/ResendConfirmationEmailRequest.cs
// DTO's/Auth/ResetPasswordRequest.cs
public record ResetPasswordRequest(string Email, string Code, string NewPassword);
