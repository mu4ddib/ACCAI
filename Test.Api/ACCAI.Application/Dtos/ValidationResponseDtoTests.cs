using ACCAI.Application.Dtos;

namespace Test.Api.ACCAI.Application.Dtos;

[TestFixture]
public class ValidationResponseDtoTests
{
    [Test]
    public void RowError_ShouldAssignAllProperties()
    {
        var err = new RowError(5, "CampoX", "Error grave", "Valor123");

        Assert.That(err.Linea, Is.EqualTo(5));
        Assert.That(err.Campo, Is.EqualTo("CampoX"));
        Assert.That(err.Mensaje, Is.EqualTo("Error grave"));
        Assert.That(err.Valor, Is.EqualTo("Valor123"));
    }

    [Test]
    public void ValidationResponseDto_From_ShouldReturnExpectedValues()
    {
        var errors = new List<RowError>
        {
            new RowError(1, "Campo", "Mensaje", "Valor")
        };

        var dto = ValidationResponseDto.From(10, errors, "cid123");

        Assert.That(dto.TotalFilas, Is.EqualTo(10));
        Assert.That(dto.Errores, Is.EqualTo(1));
        Assert.That(dto.CorrelationId, Is.EqualTo("cid123"));
        Assert.That(dto.Detalle.Count, Is.EqualTo(1));
        Assert.That(dto.Detalle[0].Campo, Is.EqualTo("Campo"));
    }

    [Test]
    public void ValidationResponseDto_Fail_ShouldReturnExpectedError()
    {
        var dto = ValidationResponseDto.Fail("Archivo vacío.", "cid456");

        Assert.That(dto.TotalFilas, Is.EqualTo(0));
        Assert.That(dto.Errores, Is.EqualTo(1));
        Assert.That(dto.CorrelationId, Is.EqualTo("cid456"));
        Assert.That(dto.Detalle, Has.Count.EqualTo(1));
        var row = dto.Detalle[0];
        Assert.That(row.Linea, Is.EqualTo(0));
        Assert.That(row.Campo, Is.EqualTo("_archivo"));
        Assert.That(row.Mensaje, Is.EqualTo("Archivo vacío."));
        Assert.That(row.Valor, Is.Null);
    }



    [Test]
    public void FailedChange_ShouldAssignAllProperties()
    {
        var fc = new FailedChange("ProdA", "ContB", "Mensaje de error");

        Assert.That(fc.Product, Is.EqualTo("ProdA"));
        Assert.That(fc.Contract, Is.EqualTo("ContB"));
        Assert.That(fc.ErrorMessage, Is.EqualTo("Mensaje de error"));
    }
}
