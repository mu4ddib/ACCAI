using System.Net;
using ACCAI.Domain.ReadModels;
using ACCAI.Infrastructure.Adapters.ExternalServices;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.Protected;

namespace Test.Api.ACCAI.Infrastructure.Adapters.ExternalServices;

[TestFixture]
public class CreaChangeFpServiceTests
{
    private Mock<ILogger<CreaChangeFpService>> _loggerMock;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<CreaChangeFpService>>();
    }

    private static HttpClient CreateHttpClient(HttpResponseMessage response)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(r => r.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(response)
            .Verifiable();

        return new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };
    }

    [Test]
    public async Task SendChangeAsync_ShouldReturnTrue_WhenResponseIsSuccess()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.OK);
        var client = CreateHttpClient(response);
        var service = new CreaChangeFpService(client, _loggerMock.Object);

        // Act
        var result = await service.SendChangeAsync(new ChangeFpItem { Contract = 1 });

        // Assert
        Assert.That(result, Is.True);
        _loggerMock.Verify(
            l => l.Log(
                It.IsAny<LogLevel>(),
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString().Contains("Sending CREA ChangeFp request")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Test]
    public async Task SendChangeAsync_ShouldReturnFalse_WhenResponseIsNotSuccess()
    {
        // Arrange
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest);
        var client = CreateHttpClient(response);
        var service = new CreaChangeFpService(client, _loggerMock.Object);

        // Act
        var result = await service.SendChangeAsync(new ChangeFpItem { Contract = 2 });

        // Assert
        Assert.That(result, Is.False);
    }

    [Test]
    public void SendChangeAsync_ShouldThrowException_WhenHttpClientFails()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new HttpRequestException("Network failure"));

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var service = new CreaChangeFpService(client, _loggerMock.Object);

        // Act & Assert
        Assert.ThrowsAsync<HttpRequestException>(async () =>
            await service.SendChangeAsync(new ChangeFpItem { Contract = 3 }));
    }
}
