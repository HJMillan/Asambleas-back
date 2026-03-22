using Asambleas.Features.Auth;
using FluentValidation;

namespace Asambleas.Features.Auth.Validators;

/// <summary>
/// Validación asíncrona para RegisterRequest.
/// Verifica unicidad de CUIL y email directamente contra la base de datos.
/// Esto se ejecuta antes de llegar al servicio, proporcionando mensajes de
/// validación claros al usuario junto con las demás validaciones síncronas.
/// </summary>
public class RegisterRequestAsyncValidator : AbstractValidator<RegisterRequest>
{
    /// <summary>
    /// Constructor que recibe el repositorio de usuarios para validación async.
    /// </summary>
    public RegisterRequestAsyncValidator(IUserRepository users)
    {
        RuleFor(x => x.Cuil)
            .MustAsync(async (cuil, ct) => !await users.ExistsByCuilAsync(cuil, ct))
            .WithMessage("Ya existe un usuario registrado con este CUIL.")
            .When(x => !string.IsNullOrEmpty(x.Cuil) && x.Cuil.Length == 11);

        RuleFor(x => x.Email)
            .MustAsync(async (email, ct) => !await users.ExistsByEmailAsync(email.Trim().ToLowerInvariant(), ct))
            .WithMessage("Ya existe un usuario registrado con este email.")
            .When(x => !string.IsNullOrEmpty(x.Email) && x.Email.Contains('@'));
    }
}
