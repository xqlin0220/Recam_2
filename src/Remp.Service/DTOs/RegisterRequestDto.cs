namespace Remp.Service.DTOs;
public class RegisterRequestDto
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public string AgentFirstName { get; set; } = string.Empty;
    public string AgentLastName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? CompanyName { get; set; }
}