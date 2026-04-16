using FluentValidation;
using Timekeeping.Api.DTOs;

namespace Timekeeping.Api.Validators;

public sealed class CreateEmployeeRequestValidator : AbstractValidator<CreateEmployeeRequest>
{
    public CreateEmployeeRequestValidator()
    {
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(32);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.UserName).NotEmpty().MaximumLength(256);
        RuleFor(x => x.Password).MinimumLength(8);
        RuleFor(x => x.Department).NotEmpty().MaximumLength(128);
        RuleFor(x => x.JobTitle).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Role).Must(r => r == "Admin" || r == "Employee");
    }
}

public sealed class UpdateMyProfileRequestValidator : AbstractValidator<UpdateMyProfileRequest>
{
    public UpdateMyProfileRequestValidator()
    {
        RuleFor(x => x.Email).EmailAddress().When(x => !string.IsNullOrWhiteSpace(x.Email));
        RuleFor(x => x.Phone).MaximumLength(50).When(x => !string.IsNullOrWhiteSpace(x.Phone));
    }
}
