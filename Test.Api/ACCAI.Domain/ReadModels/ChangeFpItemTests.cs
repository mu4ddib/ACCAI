using System.Text.Json;
using ACCAI.Domain.ReadModels;

namespace Test.Api.ACCAI.Domain.ReadModels;

[TestFixture]
public class ChangeFpItemTests
{
    [Test]
    public void Should_Assign_And_Read_All_Properties_Correctly()
    {
        // Arrange
        var item = new ChangeFpItem
        {
            PreviousAgentId = "10",
            NewAgentId = "20",
            Product = "ACCAI",
            ProductPlan = "Premium",
            Contract = "999"
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(item.PreviousAgentId, Is.EqualTo("10"));
            Assert.That(item.NewAgentId, Is.EqualTo("20"));
            Assert.That(item.Product, Is.EqualTo("ACCAI"));
            Assert.That(item.ProductPlan, Is.EqualTo("Premium"));
            Assert.That(item.Contract, Is.EqualTo("999"));
        });
    }

    [Test]
    public void Should_Serialize_And_Deserialize_With_JsonPropertyNames()
    {
        // Arrange
        var original = new ChangeFpItem
        {
            PreviousAgentId = "1",
            NewAgentId = "2",
            Product = "TestProduct",
            ProductPlan = "TestPlan",
            Contract = "123"
        };

        // Act
        var json = JsonSerializer.Serialize(original);
        var deserialized = JsonSerializer.Deserialize<ChangeFpItem>(json);

        // Assert
        Assert.That(json, Does.Contain("idAgenteAnterior"));
        Assert.That(json, Does.Contain("idAgenteNuevo"));
        Assert.That(json, Does.Contain("producto"));
        Assert.That(json, Does.Contain("planProducto"));
        Assert.That(json, Does.Contain("contrato"));

        Assert.That(deserialized!.PreviousAgentId, Is.EqualTo("1"));
        Assert.That(deserialized.NewAgentId, Is.EqualTo("2"));
        Assert.That(deserialized.Product, Is.EqualTo("TestProduct"));
        Assert.That(deserialized.ProductPlan, Is.EqualTo("TestPlan"));
        Assert.That(deserialized.Contract, Is.EqualTo("123"));
    }
}
