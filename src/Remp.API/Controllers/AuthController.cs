using Microsoft.AspNetCore.Mvc;
using Remp.Common.Utilities;
using Remp.Service.DTOs;
using Remp.Service.Interfaces;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }
    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<LoginResponseDto>>> Login(LoginRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<LoginResponseDto>.BadRequest("Validation failed", errors));
        }
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        string? userAgent = Request.Headers["User-Agent"].ToString();
        var result = await _authService.LoginAsync(request, ipAddress, userAgent);
        return Ok(ApiResponse<LoginResponseDto>.Success(result, "Login successful"));
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<RegisterResponseDto>>> Register(RegisterRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();

            return BadRequest(ApiResponse<RegisterResponseDto>.BadRequest(
                "Validation failed", errors));
        }

        // Retrieve client IP and device information (for MongoDB audit logs)
        string ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";
        string? userAgent = Request.Headers["User-Agent"].ToString();

        var result = await _authService.RegisterAsync(request, ipAddress, userAgent);

        // 201 Created = The new resource has been successfully created.
        return StatusCode(201, ApiResponse<RegisterResponseDto>.Created(
            result, "Agent registered successfully."));
    }
}

