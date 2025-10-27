using ACCAI.Domain.Exceptions;

namespace Test.Api.ACCAI.Domain.Exceptions;

[TestFixture]
public class DomainExceptionTests
{
    [Test]
    public void UnderAgeException_ShouldInheritFrom_CoreBusinessException_AndContainMessage()
    {
        // Act
        var ex = new UnderAgeException();

        // Assert
        Assert.That(ex, Is.InstanceOf<CoreBusinessException>());
        Assert.That(ex.Message, Is.EqualTo("Under age to vote."));
    }

    [Test]
    public void WrongCountryException_ShouldInheritFrom_CoreBusinessException_AndContainMessage()
    {
        // Act
        var ex = new WrongCountryException();

        // Assert
        Assert.That(ex, Is.InstanceOf<CoreBusinessException>());
        Assert.That(ex.Message, Is.EqualTo("Voter must be from Colombia."));
    }

    [Test]
    public void Exceptions_ShouldBeSerializable()
    {
        // Arrange
        var ex1 = new UnderAgeException();
        var ex2 = new WrongCountryException();

        // Act
        var serialized1 = ex1.ToString();
        var serialized2 = ex2.ToString();

        // Assert
        Assert.That(serialized1, Does.Contain("UnderAgeException"));
        Assert.That(serialized2, Does.Contain("WrongCountryException"));
    }
}
