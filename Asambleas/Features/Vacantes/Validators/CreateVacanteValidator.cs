using Asambleas.Features.Asambleas.Entities;
using Asambleas.Features.Vacantes.Entities;
using FluentValidation;

namespace Asambleas.Features.Vacantes.Validators;

/// <summary>
/// Valida el request de creación de vacante.
/// </summary>
public class CreateVacanteValidator : AbstractValidator<CreateVacanteRequest>
{
    public CreateVacanteValidator()
    {
        RuleFor(x => x.Cargo)
            .NotEmpty().WithMessage("El cargo es obligatorio.")
            .MaximumLength(200).WithMessage("El cargo no puede superar los 200 caracteres.");

        RuleFor(x => x.Nivel)
            .NotEmpty().WithMessage("El nivel es obligatorio.")
            .Must(v => Enum.TryParse<NivelAsamblea>(v, ignoreCase: true, out _))
            .WithMessage($"Nivel inválido. Valores: {string.Join(", ", Enum.GetNames<NivelAsamblea>())}");

        RuleFor(x => x.TipoCargo)
            .NotEmpty().WithMessage("El tipo de cargo es obligatorio.")
            .Must(v => Enum.TryParse<TipoCargo>(v, ignoreCase: true, out _))
            .WithMessage($"Tipo de cargo inválido. Valores: {string.Join(", ", Enum.GetNames<TipoCargo>())}");

        RuleFor(x => x.AsambleaId)
            .NotEmpty().WithMessage("La asamblea es obligatoria.");

        RuleFor(x => x.ModulosHoras)
            .GreaterThan(0).WithMessage("Los módulos/horas deben ser mayor a 0.");

        RuleFor(x => x.Turno)
            .NotEmpty().WithMessage("El turno es obligatorio.")
            .Must(v => Enum.TryParse<Turno>(v, ignoreCase: true, out _))
            .WithMessage($"Turno inválido. Valores: {string.Join(", ", Enum.GetNames<Turno>())}");

        RuleFor(x => x.Establecimiento)
            .NotNull().WithMessage("El establecimiento es obligatorio.");

        When(x => x.Establecimiento != null, () =>
        {
            RuleFor(x => x.Establecimiento.Nombre)
                .NotEmpty().WithMessage("El nombre del establecimiento es obligatorio.");
            RuleFor(x => x.Establecimiento.Direccion)
                .NotEmpty().WithMessage("La dirección del establecimiento es obligatoria.");
            RuleFor(x => x.Establecimiento.Localidad)
                .NotEmpty().WithMessage("La localidad del establecimiento es obligatoria.");
            RuleFor(x => x.Establecimiento.CodigoFuncional)
                .NotEmpty().WithMessage("El código funcional es obligatorio.");
        });
    }
}
