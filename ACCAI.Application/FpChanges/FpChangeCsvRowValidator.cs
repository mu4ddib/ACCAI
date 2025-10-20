using FluentValidation;
using System.Text.RegularExpressions;

namespace ACCAI.Application.FpChanges;

public sealed class FpChangeCsvRowValidator : AbstractValidator<FpChangeCsvRow>
{
    private static readonly Regex DigitsOnly = new(@"^\d+$", RegexOptions.Compiled);
    
    // private static readonly HashSet<string> DocTypes = new(StringComparer.OrdinalIgnoreCase)
    //     { "C", "CE", "TI", "PA", "NIT" };

    public FpChangeCsvRowValidator()
    {
        RuleFor(x => x.Nombres)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(120).WithMessage("Nombres excede 120 caracteres.");

        RuleFor(x => x.Apellidos)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(120).WithMessage("Apellidos excede 120 caracteres.");
        
        RuleFor(x => x.TipoDocum)
            .Cascade(CascadeMode.Stop)
            .Equal("C").WithMessage("Actualmente solo se admite TipoDocum = 'C'.");

        RuleFor(x => x.NroDocum)
            .Cascade(CascadeMode.Stop)
            .Must(IsDigits).WithMessage("NroDocum debe ser numérico.")
            .MaximumLength(20).WithMessage("NroDocum excede 20 caracteres.");
        
        RuleFor(x => x.Producto)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Producto es requerido.");

        RuleFor(x => x.PlanProducto)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("PlanProducto es requerido.");
        
        RuleFor(x => x.Contrato)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Contrato es requerido.")
            .Must(IsDigits).WithMessage("Contrato debe ser numérico.");
        
        RuleFor(x => x.Empresa)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(120).WithMessage("Empresa excede 120 caracteres.");

        RuleFor(x => x.Segmento)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(60).WithMessage("Segmento excede 60 caracteres.");

        RuleFor(x => x.Ciudad)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(80).WithMessage("Ciudad excede 80 caracteres.");
        
        RuleFor(x => x.IdAgte)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("IdAgte es requerido.")
            .Must(IsDigits).WithMessage("IdAgte debe ser numérico.");

        RuleFor(x => x.IdAgteNuevo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("IdAgteNuevo es requerido.")
            .Must(IsDigits).WithMessage("IdAgteNuevo debe ser numérico.")
            .Must((row, nuevo) => row.IdAgte != nuevo)
                .WithMessage("IdAgteNuevo debe ser diferente al IdAgte actual.");

        RuleFor(x => x.NombreAgteNuevo)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("NombreAgteNuevo es requerido.")
            .MaximumLength(150).WithMessage("NombreAgteNuevo excede 150 caracteres.");

        
        RuleFor(x => x.descripcion)
            .Cascade(CascadeMode.Stop)
            .MaximumLength(250).WithMessage("descripcion excede 250 caracteres.");
    }

    private static bool IsDigits(string? s) =>
        !string.IsNullOrWhiteSpace(s) && DigitsOnly.IsMatch(s);
}
