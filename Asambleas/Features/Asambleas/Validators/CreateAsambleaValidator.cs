using Asambleas.Features.Asambleas.Entities;
using FluentValidation;

namespace Asambleas.Features.Asambleas.Validators;

/// <summary>
/// Valida el request de creación de asamblea.
/// </summary>
public class CreateAsambleaValidator : AbstractValidator<CreateAsambleaRequest>
{
    public CreateAsambleaValidator()
    {
        RuleFor(x => x.Fecha)
            .NotEmpty().WithMessage("La fecha es obligatoria.")
            .Must(BeAValidDate).WithMessage("Formato de fecha inválido. Use yyyy-MM-dd.");

        RuleFor(x => x.Nivel)
            .NotEmpty().WithMessage("El nivel es obligatorio.")
            .Must(BeAValidNivel).WithMessage(
                $"Nivel inválido. Valores: {string.Join(", ", Enum.GetNames<NivelAsamblea>())}");

        RuleFor(x => x.Tipo)
            .NotEmpty().WithMessage("El tipo es obligatorio.")
            .Must(BeAValidTipo).WithMessage(
                $"Tipo inválido. Valores: {string.Join(", ", Enum.GetNames<TipoAsamblea>())}");

        RuleFor(x => x.HoraInicio)
            .NotEmpty().WithMessage("La hora de inicio es obligatoria.");

        RuleFor(x => x.HoraFin)
            .NotEmpty().WithMessage("La hora de fin es obligatoria.");

        RuleFor(x => x.VentanaInscripcionInicio)
            .NotEmpty().WithMessage("El inicio de la ventana de inscripción es obligatorio.");

        RuleFor(x => x.VentanaInscripcionFin)
            .NotEmpty().WithMessage("El fin de la ventana de inscripción es obligatorio.");

        RuleFor(x => x.Lugar)
            .NotEmpty().WithMessage("El lugar es obligatorio.")
            .MaximumLength(500).WithMessage("El lugar no puede superar los 500 caracteres.");
    }

    private static bool BeAValidDate(string fecha)
        => DateOnly.TryParse(fecha, out _);

    private static bool BeAValidNivel(string nivel)
        => Enum.TryParse<NivelAsamblea>(nivel, ignoreCase: true, out _);

    private static bool BeAValidTipo(string tipo)
        => Enum.TryParse<TipoAsamblea>(tipo, ignoreCase: true, out _);
}
