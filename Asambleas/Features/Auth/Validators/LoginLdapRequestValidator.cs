using FluentValidation;

namespace Asambleas.Features.Auth.Validators;

/// <summary>
/// Valida el request de login LDAP.
/// </summary>
public class LoginLdapRequestValidator : AbstractValidator<LoginLdapRequest>
{
    public LoginLdapRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("El usuario es obligatorio.")
            .MaximumLength(100).WithMessage("El usuario no puede superar los 100 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MaximumLength(128).WithMessage("La contraseña no puede superar los 128 caracteres.");
    }
}
