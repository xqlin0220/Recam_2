using FluentValidation;
using Remp.Service.DTOs;

namespace Remp.Service.Validators;
public class RegisterRequestValidator : AbstractValidator<RegisterRequestDto>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()                                          
                .WithMessage("Email is required.")           
            .EmailAddress()                                      
                .WithMessage("Invalid email format.")
            .MaximumLength(200)                                 
                .WithMessage("Email must not exceed 50 characters.");

        RuleFor(x => x.Password)
            .NotEmpty()
                .WithMessage("Password is required.")
            .MinimumLength(8)                  
                .WithMessage("Password must be at least 8 characters.")
            .MaximumLength(100)
                .WithMessage("Password must not exceed 50 characters.")
            .Matches("[A-Z]")
                .WithMessage("Password must contain at least one uppercase letter.")
            .Matches("[0-9]")
                .WithMessage("Password must contain at least one number.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty()
                .WithMessage("Confirm password is required.")
            .Equal(x => x.Password)
                .WithMessage("Passwords do not match.");

        RuleFor(x => x.AgentFirstName)
            .NotEmpty()
                .WithMessage("First name is required.")
            .MaximumLength(100)
                .WithMessage("First name must not exceed 100 characters.");

        RuleFor(x => x.AgentLastName)
            .NotEmpty()
                .WithMessage("Last name is required.")
            .MaximumLength(100)
                .WithMessage("Last name must not exceed 100 characters.");

        RuleFor(x => x.AvatarUrl)
            .Must(BeAValidUrl)                               
                .WithMessage("Avatar URL must be a valid URL.")
            .When(x => !string.IsNullOrEmpty(x.AvatarUrl));   
    }

    private static bool BeAValidUrl(string? url)
    {
        if (string.IsNullOrEmpty(url)) return true;
        return Uri.TryCreate(url, UriKind.Absolute, out _);
    }
}