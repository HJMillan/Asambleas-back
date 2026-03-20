using FluentValidation;

namespace Asambleas.Features.Auth.Validators;

/// <summary>
/// Valida el request de login por CUIL.
/// </summary>
public class LoginCuilRequestValidator : AbstractValidator<LoginCuilRequest>
{
    public LoginCuilRequestValidator()
    {
        RuleFor(x => x.Cuil)
            .NotEmpty().WithMessage("El CUIL es obligatorio.")
            .Length(11).WithMessage("El CUIL debe tener exactamente 11 caracteres.")
            .Matches(@"^\d{11}$").WithMessage("El CUIL debe contener solo dígitos.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MaximumLength(128).WithMessage("La contraseña no puede superar los 128 caracteres.");
    }
}
