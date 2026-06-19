using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Remp.Common.Exceptions;
using Remp.Common.Helpers;
using Remp.Common.Utilities;
using Remp.DataAccess.Collections;
using Remp.Models.Entities;
using Remp.Service.DTOs;
using Remp.Service.Interfaces;

namespace Remp.Service.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly MongoDbContext _mongoDbContext;
    public AuthService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IOptions<JwtSettings> jwtSettings,
        MongoDbContext mongoDbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
        _mongoDbContext = mongoDbContext;
    }
    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress, string? userAgent)
    {
        // Check if the user exists
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            await LogLoginAttemptAsync(
                request.Email, isSuccess: false,
                failureReason: "User not found",
                userId: null, role: null,
                ipAddress, userAgent);
            throw new UnauthorizedException("Invalid email or password.");
        }
        // Verify password
        var signInResult = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!signInResult.Succeeded)
        {
            string failureReason = signInResult.IsLockedOut ?
                "Account locked due to multiple failed attempts" : "Invalid password";

            await LogLoginAttemptAsync(
                request.Email, isSuccess: false,
                failureReason: failureReason,
                userId: user.Id, role: null,
                ipAddress, userAgent);

            throw new UnauthorizedException(signInResult.IsLockedOut? "Account is locked. Please try again later.": "Invalid email or password.");
        }
        // Obtain user roles (distinguish between Admin / Agent)
        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            // Exception: The user exists but has not been assigned a role (data issues)
            await LogLoginAttemptAsync(
                request.Email, isSuccess: false,
                failureReason: "User has no assigned role",
                userId: user.Id, role: null,
                ipAddress, userAgent);

            throw new UnauthorizedException("Account configuration error. Please contact support.");
        }
        // Take the first character as the main character
        var primaryRole = roles.First();
        // Generate JWT Tokens
        var token = JwtTokenHelper.GenerateToken(user.Id, user.Email!, roles, _jwtSettings);
        var expiresAt = JwtTokenHelper.GetExpiryTime(_jwtSettings);
        // Display names based on the character
        string displayName = await GetDisplayNameAsync(user.Id, primaryRole);
        // Record the successful login log
        await LogLoginAttemptAsync(
            request.Email, isSuccess: true,
            failureReason: null,
            userId: user.Id, role: primaryRole,
            ipAddress, userAgent);

        // Return results
        return new LoginResponseDto
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            Email = user.Email!,
            Role = primaryRole,
            DisplayName = displayName
        };
    }
    // Check different tables based on role to get display names
    // add async after method complete
    private Task<string> GetDisplayNameAsync(string userId, string role)
    {
        // TODO: Inject the corresponding repository from PhotographyCompany or Agent table to check
        if (role == "Agent")
        {
            return Task.FromResult("Agent Name");
        }
        else if (role == "PhotographyCompany")
        {
            return Task.FromResult("Company Name");
        }
        return Task.FromResult(userId);
    }
    // Write logs to MongoDB
    private async Task LogLoginAttemptAsync(
        string email, bool isSuccess, string? failureReason,
        string? userId, string? role,
        string ipAddress, string? userAgent)
    {
        var log = new LoginAttemptLog
        {
            Email = email,
            IsSuccess = isSuccess,
            FailureReason = failureReason,
            UserId = userId,
            Role = role,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            AttemptedAt = DateTime.UtcNow
        };
        await _mongoDbContext.LoginAttemptLogs.InsertOneAsync(log);
    }
}