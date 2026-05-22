namespace BloodConnectAPI.Models.DTOs;

public class RoleDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class CreateRoleDto
{
    public string Name { get; set; } = string.Empty;
}

public class AssignRoleDto
{
    public string UserId { get; set; } = string.Empty;
    public string RoleName { get; set; } = string.Empty;
}
