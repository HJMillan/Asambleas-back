using Asambleas.Features.Auth.Entities;
using FluentValidation;

namespace Asambleas.Features.Usuarios.Validators;

/// <summary>
/// Valida que el rol enviado sea un valor válido del enum Role.
/// </summary>
public class UpdateUserRoleValidator : AbstractValidator<UpdateUserRoleRequest>
{
    public UpdateUserRoleValidator()
    {
        RuleFor(x => x.Role)
            .NotEmpty().WithMessage("El rol es obligatorio.")
            .Must(BeAValidRole).WithMessage(
                $"Rol inválido. Valores permitidos: {string.Join(", ", Enum.GetNames<Role>())}");
    }

    private static bool BeAValidRole(string role)
        => Enum.TryParse<Role>(role, ignoreCase: false, out _);
}
