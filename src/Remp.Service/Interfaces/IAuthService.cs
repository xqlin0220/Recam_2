using Remp.Service.DTOs;

namespace Remp.Service.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, string? userAgent);
    Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, string ipAddress, string? userAgent);
}