using ACCAI.Application.FpChanges;

namespace Test.Api.ACCAI.Application.FpChanges;

[TestFixture]
public class ProcessResponseDtoTests
{
    [Test]
    public void Constructor_Should_Assign_All_Properties()
    {
        // Act
        var dto = new ProcessResponseDto(100, 80, 20);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(dto.Total, Is.EqualTo(100));
            Assert.That(dto.Procesados, Is.EqualTo(80));
            Assert.That(dto.Rechazados, Is.EqualTo(20));
        });
    }

    [Test]
    public void Records_Should_Be_Equal_When_Properties_Are_Identical()
    {
        var dto1 = new ProcessResponseDto(10, 5, 5);
        var dto2 = new ProcessResponseDto(10, 5, 5);

        Assert.Multiple(() =>
        {
            Assert.That(dto1, Is.EqualTo(dto2));
            Assert.That(dto1 == dto2, Is.True);
            Assert.That(dto1 != dto2, Is.False);
            Assert.That(dto1.GetHashCode(), Is.EqualTo(dto2.GetHashCode()));
        });
    }

    [Test]
    public void Records_Should_Not_Be_Equal_When_Properties_Differ()
    {
        var dto1 = new ProcessResponseDto(10, 5, 5);
        var dto2 = dto1 with { Procesados = 6 };

        Assert.Multiple(() =>
        {
            Assert.That(dto1, Is.Not.EqualTo(dto2));
            Assert.That(dto1 == dto2, Is.False);
            Assert.That(dto1 != dto2, Is.True);
        });
    }

    [Test]
    public void ToString_Should_Return_NonEmpty_String()
    {
        var dto = new ProcessResponseDto(10, 5, 5);
        var str = dto.ToString();

        Assert.Multiple(() =>
        {
            Assert.That(str, Is.Not.Null.And.Not.Empty);
            Assert.That(str, Does.Contain("Total = 10"));
        });
    }

    [Test]
    public void With_Should_Create_Copy_With_Modified_Values()
    {
        var original = new ProcessResponseDto(10, 5, 5);
        var modified = original with { Rechazados = 2 };

        Assert.Multiple(() =>
        {
            Assert.That(modified, Is.Not.EqualTo(original));
            Assert.That(modified.Total, Is.EqualTo(original.Total));
            Assert.That(modified.Rechazados, Is.EqualTo(2));
        });
    }
}
