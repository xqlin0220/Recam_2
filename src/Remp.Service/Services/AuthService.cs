using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Remp.Common.Exceptions;
using Remp.Common.Helpers;
using Remp.Common.Utilities;
using Remp.DataAccess.Collections;
using Remp.DataAccess.Data;
using Remp.Models.Entities;
using Remp.Service.DTOs;
using Remp.Service.Interfaces;
using FluentValidation; 

namespace Remp.Service.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<AppUser> _userManager;
    private readonly SignInManager<AppUser> _signInManager;
    private readonly JwtSettings _jwtSettings;
    private readonly MongoDbContext _mongoDbContext;
    private readonly IValidator<RegisterRequestDto> _registerValidator;
    private readonly AppDbContext _dbContext;
    public AuthService(
        UserManager<AppUser> userManager,
        SignInManager<AppUser> signInManager,
        IOptions<JwtSettings> jwtSettings,
        MongoDbContext mongoDbContext,
        IValidator<RegisterRequestDto> registerValidator, 
        AppDbContext dbContext)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtSettings = jwtSettings.Value;
        _mongoDbContext = mongoDbContext;
        _registerValidator = registerValidator;   
        _dbContext = dbContext;  
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

    public async Task<RegisterResponseDto> RegisterAsync(
    RegisterRequestDto request, string ipAddress, string? userAgent)
{
    // 1. Trigger FluentValidation verification  
    // inject IValidator and add this dependency to the constructor
    var validationResult = await _registerValidator.ValidateAsync(request);

    // validationResult.IsValid = false
    if (!validationResult.IsValid)
    {
        // Collect all validation error information into one List<string>
        var validationErrors = validationResult.Errors
            .Select(e => e.ErrorMessage)
            .ToList(); 

        // Record failure logs to MongoDB
        await LogRegistrationAsync(
            request.Email, isSuccess: false,
            failureReason: string.Join("; ", validationErrors),
            userId: null, role: null,
            ipAddress, userAgent);

        // GlobalExceptionMiddleware will automatically capture and return 400
        throw new Remp.Common.Exceptions.ValidationException(validationErrors);
    }
    // 2. Check if the mailbox is registered
    // Search for users by email address.
    var existingUser = await _userManager.FindByEmailAsync(request.Email);

    if (existingUser != null)
    {
        // The email already exists, and a failure log is recorded
        await LogRegistrationAsync(
            request.Email, isSuccess: false,
            failureReason: "Email already registered",
            userId: null, role: null,
            ipAddress, userAgent);

        // 409 Conflict: The resource already exists.
        throw new Remp.Common.Exceptions.ValidationException(
            new List<string> { "This email is already registered." });
    }

    // 3.Create an AppUser object
    var newUser = new Remp.Models.Entities.AppUser
    {
        UserName = request.Email,
        Email = request.Email,

        EmailConfirmed = true,

        CreatedAt = DateTime.UtcNow,
        IsDeleted = false
    };

    // 4.Create a user (automatic hash password) with UserManager
    var createResult = await _userManager.CreateAsync(newUser, request.Password);

    if (!createResult.Succeeded)
    {
        var identityErrors = createResult.Errors
            .Select(e => e.Description)
            .ToList();

        await LogRegistrationAsync(
            request.Email, isSuccess: false,
            failureReason: string.Join("; ", identityErrors),
            userId: null, role: null,
            ipAddress, userAgent);

        throw new Remp.Common.Exceptions.ValidationException(identityErrors);
    }

    // 5.Assign Agent roles
    var roleResult = await _userManager.AddToRoleAsync(
        newUser, Remp.Common.Constants.Roles.Agent);

    if (!roleResult.Succeeded)
    {
        // Role assignment failed, and rollback is needed: delete the newly created user
        await _userManager.DeleteAsync(newUser);

        await LogRegistrationAsync(
            request.Email, isSuccess: false,
            failureReason: "Failed to assign Agent role",
            userId: newUser.Id, role: null,
            ipAddress, userAgent);

        throw new Exception("Failed to assign role. Registration rolled back.");
    }

    // 6. Insert Profile information into the Agent table
    var agentProfile = new Remp.Models.Entities.Agent
    {
        Id = newUser.Id,
        AgentFirstName = request.AgentFirstName,
        AgentLastName = request.AgentLastName,
        AvatarUrl = request.AvatarUrl,
        CompanyName = request.CompanyName
    };

    await _dbContext.Agents.AddAsync(agentProfile);
    await _dbContext.SaveChangesAsync();  

    // 7. Record successful logs to MongoDB 
    await LogRegistrationAsync(
        request.Email, isSuccess: true,
        failureReason: null,
        userId: newUser.Id,
        role: Remp.Common.Constants.Roles.Agent,
        ipAddress, userAgent);

    // 8. Return results 
    return new RegisterResponseDto
    {
        UserId = newUser.Id,
        Email = newUser.Email!,
        Role = Remp.Common.Constants.Roles.Agent,
        FullName = $"{request.AgentFirstName} {request.AgentLastName}",
        CreatedAt = newUser.CreatedAt
    };
}

// Private helper method: Write registration logs to MongoDB.
private async Task LogRegistrationAsync(
    string email, bool isSuccess, string? failureReason,
    string? userId, string? role,
    string ipAddress, string? userAgent)
    {
        var log = new Remp.Models.Entities.RegistrationLog
        {
            Email = email,
            IsSuccess = isSuccess,
            FailureReason = failureReason,
            UserId = userId,
            Role = role,
            IpAddress = ipAddress,
            UserAgent = userAgent,
            RegisteredAt = DateTime.UtcNow
        };

        await _mongoDbContext.RegistrationLogs.InsertOneAsync(log);
    }
}