using FluentValidation;
using Timekeeping.Api.DTOs;

namespace Timekeeping.Api.Validators;

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty();
    }
}
