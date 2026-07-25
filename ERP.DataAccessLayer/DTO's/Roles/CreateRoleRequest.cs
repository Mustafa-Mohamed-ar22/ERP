public record CreateRoleRequest(string Name, string? Description, List<string> PermissionCodes);
