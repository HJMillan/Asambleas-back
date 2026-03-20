using FluentValidation;

namespace Asambleas.Features.Auth.Validators;

/// <summary>
/// Valida el request de registro de usuario.
/// Centraliza la validación que antes estaba inline en AuthService.
/// </summary>
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Cuil)
            .NotEmpty().WithMessage("El CUIL es obligatorio.")
            .Length(11).WithMessage("El CUIL debe tener exactamente 11 caracteres.")
            .Matches(@"^\d{11}$").WithMessage("El CUIL debe contener solo dígitos.");

        RuleFor(x => x.Nombre)
            .NotEmpty().WithMessage("El nombre es obligatorio.")
            .MaximumLength(100).WithMessage("El nombre no puede superar los 100 caracteres.");

        RuleFor(x => x.Apellido)
            .NotEmpty().WithMessage("El apellido es obligatorio.")
            .MaximumLength(100).WithMessage("El apellido no puede superar los 100 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("El email es obligatorio.")
            .EmailAddress().WithMessage("El formato del email es inválido.")
            .MaximumLength(256).WithMessage("El email no puede superar los 256 caracteres.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("La contraseña es obligatoria.")
            .MinimumLength(8).WithMessage("La contraseña debe tener al menos 8 caracteres.")
            .MaximumLength(128).WithMessage("La contraseña no puede superar los 128 caracteres.")
            .Matches(@"[A-Z]").WithMessage("La contraseña debe contener al menos una mayúscula.")
            .Matches(@"[a-z]").WithMessage("La contraseña debe contener al menos una minúscula.")
            .Matches(@"\d").WithMessage("La contraseña debe contener al menos un número.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("La confirmación de contraseña es obligatoria.")
            .Equal(x => x.Password).WithMessage("Las contraseñas no coinciden.");
    }
}
