using ACCAI.Application.FpChanges;

namespace Test.Api.ACCAI.Application.FpChanges;

[TestFixture]
public class FpChangeCsvRowTests
{
    [Test]
    public void Constructor_Should_Assign_All_Properties_Correctly()
    {
        // Arrange
        var row = new FpChangeCsvRow(
            "Pérez",            // Apellidos
            "Juan",             // Nombres
            "12345",            // NroDocum
            "C",                // TipoDocum
            "ProductoA",        // Producto
            "PlanA",            // PlanProducto
            "9876",             // Contrato
            "EmpresaX",         // Empresa
            "Segmento1",        // Segmento
            "Bogotá",           // Ciudad
            "111",              // IdAgte
            "222",              // IdAgteNuevo
            "Nuevo Agente",     // NombreAgteNuevo
            "SubGrupo",         // SubGrupoFp
            "Descripción demo"  // descripcion
        );

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(row.Apellidos, Is.EqualTo("Pérez"));
            Assert.That(row.Nombres, Is.EqualTo("Juan"));
            Assert.That(row.NroDocum, Is.EqualTo("12345"));
            Assert.That(row.TipoDocum, Is.EqualTo("C"));
            Assert.That(row.Producto, Is.EqualTo("ProductoA"));
            Assert.That(row.PlanProducto, Is.EqualTo("PlanA"));
            Assert.That(row.Contrato, Is.EqualTo("9876"));
            Assert.That(row.Empresa, Is.EqualTo("EmpresaX"));
            Assert.That(row.Segmento, Is.EqualTo("Segmento1"));
            Assert.That(row.Ciudad, Is.EqualTo("Bogotá"));
            Assert.That(row.IdAgte, Is.EqualTo("111"));
            Assert.That(row.IdAgteNuevo, Is.EqualTo("222"));
            Assert.That(row.NombreAgteNuevo, Is.EqualTo("Nuevo Agente"));
            Assert.That(row.SubGrupoFp, Is.EqualTo("SubGrupo"));
            Assert.That(row.descripcion, Is.EqualTo("Descripción demo"));
        });
    }

    [Test]
    public void Records_Should_Be_Equal_When_Properties_Are_Identical()
    {
        // Arrange
        var row1 = new FpChangeCsvRow("A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O");
        var row2 = new FpChangeCsvRow("A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O");

        // Act & Assert
        Assert.That(row1, Is.EqualTo(row2));
        Assert.That(row1 == row2, Is.True);
        Assert.That(row1 != row2, Is.False);
        Assert.That(row1.GetHashCode(), Is.EqualTo(row2.GetHashCode()));
    }

    [Test]
    public void Records_Should_Not_Be_Equal_When_Properties_Differ()
    {
        var row1 = new FpChangeCsvRow("A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O");
        var row2 = row1 with { Nombres = "Otro" };

        Assert.That(row1, Is.Not.EqualTo(row2));
        Assert.That(row1 == row2, Is.False);
        Assert.That(row1 != row2, Is.True);
    }

    [Test]
    public void ToString_Should_Return_NonEmpty_String()
    {
        var row = new FpChangeCsvRow("A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O");
        var str = row.ToString();

        Assert.That(str, Is.Not.Null.And.Not.Empty);
        Assert.That(str, Does.Contain("Apellidos = A"));
    }

    [Test]
    public void With_Should_Create_Copy_With_Modified_Values()
    {
        var original = new FpChangeCsvRow("A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O");
        var modified = original with { Ciudad = "Nueva Ciudad", descripcion = "Cambio" };

        Assert.That(modified, Is.Not.EqualTo(original));
        Assert.That(modified.Ciudad, Is.EqualTo("Nueva Ciudad"));
        Assert.That(modified.descripcion, Is.EqualTo("Cambio"));
        Assert.That(modified.Apellidos, Is.EqualTo(original.Apellidos)); // unchanged
    }
}