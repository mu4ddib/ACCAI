using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ACCAI.Infrastructure.Parsers;

namespace Test.Api.ACCAI.Infrastructure.Parsers
{
    [TestFixture]
    public class FpChangeCsvParserTests
    {
        [Test]
        public async Task ParseAsync_ShouldParseCsvAndReturnExpectedResult()
        {
            // Arrange: crear CSV con encabezado y una fila
            var csvContent = string.Join(",", new[]
            {
            "Apellidos","Nombres","NroDocum","TipoDocum","Producto",
            "PlanProducto","Contrato","Empresa","Segmento","Ciudad",
            "IdAgte","IdAgteNuevo","NombreAgteNuevo","SubGrupoFp","descripcion"
        }) + "\n" +
            "Perez,Gabriela,123,CC,VIDA,Plan A,101,ACME,Empresarial,Bogota,10,20,Juan Perez,SubA,Descripcion test";

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
            var parser = new FpChangeCsvParser();

            // Act
            var result = await parser.ParseAsync(stream);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Header, Is.Not.Empty);
            Assert.That(result.Rows.Count, Is.EqualTo(1));

            var row = result.Rows.First();

            Assert.That(row.Apellidos, Is.EqualTo("Perez"));
            Assert.That(row.Nombres, Is.EqualTo("Gabriela"));
            Assert.That(row.NroDocum, Is.EqualTo("123"));
            Assert.That(row.TipoDocum, Is.EqualTo("CC"));
            Assert.That(row.Producto, Is.EqualTo("VIDA"));
            Assert.That(row.PlanProducto, Is.EqualTo("Plan A"));
            Assert.That(row.Contrato, Is.EqualTo("101"));
            Assert.That(row.Empresa, Is.EqualTo("ACME"));
            Assert.That(row.Segmento, Is.EqualTo("Empresarial"));
            Assert.That(row.Ciudad, Is.EqualTo("Bogota"));
            Assert.That(row.IdAgte, Is.EqualTo("10"));
            Assert.That(row.IdAgteNuevo, Is.EqualTo("20"));
            Assert.That(row.NombreAgteNuevo, Is.EqualTo("Juan Perez"));
            Assert.That(row.SubGrupoFp, Is.EqualTo("SubA"));
            Assert.That(row.descripcion, Is.EqualTo("Descripcion test"));
        }

        [Test]
        public async Task ParseAsync_ShouldHandleEmptyFieldsAndNulls()
        {
            // Arrange: CSV con campos vacíos y delimitador estándar
            var csvContent = "Apellidos,Nombres,NroDocum,TipoDocum,Producto,PlanProducto,Contrato,Empresa,Segmento,Ciudad,IdAgte,IdAgteNuevo,NombreAgteNuevo,SubGrupoFp,descripcion\n" +
                             ",,,,,,,,,,,,,,"; // fila vacía

            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
            var parser = new FpChangeCsvParser();

            // Act
            var result = await parser.ParseAsync(stream);

            // Assert
            Assert.That(result.Header, Is.Not.Empty);
            Assert.That(result.Rows, Is.Not.Empty);

            var row = result.Rows.First();
            Assert.Multiple(() =>
            {
                Assert.That(row.Apellidos, Is.EqualTo(string.Empty));
                Assert.That(row.Nombres, Is.EqualTo(string.Empty));
                Assert.That(row.NroDocum, Is.EqualTo(string.Empty));
                Assert.That(row.Producto, Is.EqualTo(string.Empty));
                Assert.That(row.descripcion, Is.EqualTo(string.Empty));
            });
        }

        [Test]
        public async Task ParseAsync_ShouldResetStreamPosition_WhenSeekable()
        {
            // Arrange
            var csvContent = "Apellidos,Nombres\nTest,User";
            var stream = new MemoryStream(Encoding.UTF8.GetBytes(csvContent));
            stream.Position = 5; // posición no inicial

            var parser = new FpChangeCsvParser();

            // Act
            var result = await parser.ParseAsync(stream);

            // Assert: debería haber reseteado la posición y leído correctamente
            Assert.That(result.Rows.Count, Is.EqualTo(1));
            Assert.That(result.Rows.First().Apellidos, Is.EqualTo("Test"));
        }
    }
}
