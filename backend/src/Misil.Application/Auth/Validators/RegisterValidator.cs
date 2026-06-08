using FluentValidation;
using Misil.Application.Auth.Commands;

namespace Misil.Application.Auth.Validators;
public class RegisterValidator : AbstractValidator<RegisterCommand>
{
    public RegisterValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty().MinimumLength(6);
        RuleFor(x => x.Username).NotEmpty().MinimumLength(3);
    }
}
