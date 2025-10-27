using ACCAI.Application.FpChanges;
using FluentValidation.TestHelper;

namespace Test.Api.ACCAI.Application.FpChanges;

[TestFixture]
public class FpChangeCsvRowValidatorTests
{
    private FpChangeCsvRowValidator _validator;

    [SetUp]
    public void Setup() => _validator = new FpChangeCsvRowValidator();

    private static FpChangeCsvRow CreateValidRow() => new(
        "Pérez",           // Apellidos
        "Juan",            // Nombres
        "12345",           // NroDocum
        "C",               // TipoDocum
        "ProductoA",       // Producto
        "PlanA",           // PlanProducto
        "9876",            // Contrato
        "EmpresaX",        // Empresa
        "Segmento1",       // Segmento
        "Bogotá",          // Ciudad
        "111",             // IdAgte
        "222",             // IdAgteNuevo
        "Nuevo Agente",    // NombreAgteNuevo
        "Subgrupo",        // SubGrupoFp
        "descripcion test" // descripcion
    );

    [Test]
    public void Should_Pass_Validation_When_All_Fields_Are_Valid()
    {
        var model = CreateValidRow();
        var result = _validator.TestValidate(model);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public void Should_Fail_When_Nombres_Too_Long()
    {
        var model = CreateValidRow() with { Nombres = new string('a', 121) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Nombres)
            .WithErrorMessage("Nombres excede 120 caracteres.");
    }

    [Test]
    public void Should_Fail_When_Apellidos_Too_Long()
    {
        var model = CreateValidRow() with { Apellidos = new string('b', 121) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Apellidos)
            .WithErrorMessage("Apellidos excede 120 caracteres.");
    }

    [Test]
    public void Should_Fail_When_TipoDocum_Not_C()
    {
        var model = CreateValidRow() with { TipoDocum = "P" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.TipoDocum)
            .WithErrorMessage("Actualmente solo se admite TipoDocum = 'C'.");
    }

    [Test]
    public void Should_Fail_When_NroDocum_NotNumeric_Or_TooLong()
    {
        var model = CreateValidRow() with { NroDocum = "ABC" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NroDocum)
            .WithErrorMessage("NroDocum debe ser numérico.");

        model = CreateValidRow() with { NroDocum = new string('9', 21) };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NroDocum)
            .WithErrorMessage("NroDocum excede 20 caracteres.");
    }

    [Test]
    public void Should_Fail_When_Producto_Or_PlanProducto_Empty()
    {
        var model = CreateValidRow() with { Producto = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Producto)
            .WithErrorMessage("Producto es requerido.");

        model = CreateValidRow() with { PlanProducto = "" };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.PlanProducto)
            .WithErrorMessage("PlanProducto es requerido.");
    }

    [Test]
    public void Should_Fail_When_Contrato_Empty_Or_NotNumeric()
    {
        var model = CreateValidRow() with { Contrato = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Contrato)
            .WithErrorMessage("Contrato es requerido.");

        model = CreateValidRow() with { Contrato = "abc" };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Contrato)
            .WithErrorMessage("Contrato debe ser numérico.");
    }

    [Test]
    public void Should_Fail_When_Empresa_Segmento_Ciudad_TooLong()
    {
        var model = CreateValidRow() with { Empresa = new string('E', 121) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Empresa)
            .WithErrorMessage("Empresa excede 120 caracteres.");

        model = CreateValidRow() with { Segmento = new string('S', 61) };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Segmento)
            .WithErrorMessage("Segmento excede 60 caracteres.");

        model = CreateValidRow() with { Ciudad = new string('C', 81) };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.Ciudad)
            .WithErrorMessage("Ciudad excede 80 caracteres.");
    }

    [Test]
    public void Should_Fail_When_IdAgte_Or_IdAgteNuevo_Invalid()
    {
        // IdAgte vacío
        var model = CreateValidRow() with { IdAgte = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.IdAgte)
            .WithErrorMessage("IdAgte es requerido.");

        // IdAgte no numérico
        model = CreateValidRow() with { IdAgte = "abc" };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.IdAgte)
            .WithErrorMessage("IdAgte debe ser numérico.");

        // IdAgteNuevo vacío
        model = CreateValidRow() with { IdAgteNuevo = "" };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.IdAgteNuevo)
            .WithErrorMessage("IdAgteNuevo es requerido.");

        // IdAgteNuevo no numérico
        model = CreateValidRow() with { IdAgteNuevo = "xyz" };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.IdAgteNuevo)
            .WithErrorMessage("IdAgteNuevo debe ser numérico.");

        // IdAgteNuevo igual a IdAgte
        model = CreateValidRow() with { IdAgte = "111", IdAgteNuevo = "111" };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.IdAgteNuevo)
            .WithErrorMessage("IdAgteNuevo debe ser diferente al IdAgte actual.");
    }

    [Test]
    public void Should_Fail_When_NombreAgteNuevo_Empty_Or_TooLong()
    {
        var model = CreateValidRow() with { NombreAgteNuevo = "" };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NombreAgteNuevo)
            .WithErrorMessage("NombreAgteNuevo es requerido.");

        model = CreateValidRow() with { NombreAgteNuevo = new string('N', 151) };
        result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.NombreAgteNuevo)
            .WithErrorMessage("NombreAgteNuevo excede 150 caracteres.");
    }

    [Test]
    public void Should_Fail_When_Descripcion_TooLong()
    {
        var model = CreateValidRow() with { descripcion = new string('d', 251) };
        var result = _validator.TestValidate(model);
        result.ShouldHaveValidationErrorFor(x => x.descripcion)
            .WithErrorMessage("descripcion excede 250 caracteres.");
    }

    [Test]
    public void IsDigits_Should_Work_For_Valid_And_Invalid_Cases()
    {
        // Método privado probado indirectamente
        var method = typeof(FpChangeCsvRowValidator)
            .GetMethod("IsDigits", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        Assert.That(method!.Invoke(null, new object?[] { "12345" }), Is.EqualTo(true));
        Assert.That(method!.Invoke(null, new object?[] { "abc" }), Is.EqualTo(false));
        Assert.That(method!.Invoke(null, new object?[] { "" }), Is.EqualTo(false));
        Assert.That(method!.Invoke(null, new object?[] { null }), Is.EqualTo(false));
    }
}