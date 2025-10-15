using FluentValidation;
namespace ACCAI.Application.FpChanges;

public sealed class FpChangeCsvRowValidator : AbstractValidator<FpChangeCsvRow>
{
    public FpChangeCsvRowValidator()
    {
        // Campos requeridos (sin espacios)
        RuleFor(x => x.Contrato).NotEmpty().Must(IsDigits).WithMessage("Contrato debe ser numérico.");
        RuleFor(x => x.Producto).NotEmpty().Equal("ACCAI")
            .WithMessage("Producto debe ser exactamente 'ACCAI'.");
        RuleFor(x => x.PlanProducto).NotEmpty().Must(IsDigits).WithMessage("PlanProducto debe ser numérico.");
        RuleFor(x => x.NroDocum).NotEmpty().Must(IsDigits).WithMessage("NroDocum debe ser numérico.");
        RuleFor(x => x.TipoDocum).NotEmpty();
        RuleFor(x => x.IdAgteNuevo).NotEmpty().Must(IsDigits).WithMessage("IdAgteNuevo debe ser numérico.");
        RuleFor(x => x.IdAgte).NotEmpty().Must(IsDigits).WithMessage("IdAgte debe ser numérico.");
        RuleFor(x => x.SubGrupoFp).NotEmpty().Must(IsDigits).WithMessage("SubGrupoFp debe ser numérico.");
        RuleFor(x => x.MotivoCambio).NotEmpty();
    }

    private static bool IsDigits(string? s) => !string.IsNullOrWhiteSpace(s) && s.All(char.IsDigit);
}
