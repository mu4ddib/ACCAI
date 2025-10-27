using ACCAI.Domain.ReadModels;

namespace Test.Api.ACCAI.Domain.ReadModels;

[TestFixture]
public class AgentChangeItemTests
{
    [Test]
    public void Should_Assign_And_Read_All_Properties_Correctly()
    {
        // Arrange
        var item = new AgentChangeItem
        {
            OldAgentId = 11,
            NewAgentId = 22,
            ContractNumber = 333
        };

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(item.OldAgentId, Is.EqualTo(11));
            Assert.That(item.NewAgentId, Is.EqualTo(22));
            Assert.That(item.ContractNumber, Is.EqualTo(333));
        });
    }

    [Test]
    public void Should_Support_Default_Construction()
    {
        // Act
        var item = new AgentChangeItem();

        // Assert: default values
        Assert.That(item.OldAgentId, Is.EqualTo(0));
        Assert.That(item.NewAgentId, Is.EqualTo(0));
        Assert.That(item.ContractNumber, Is.EqualTo(0));
    }
}