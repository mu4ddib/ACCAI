using ACCAI.Application.Common;
using ACCAI.Domain.ReadModels;
using ACCAI.Infrastructure.Adapters.ExternalServices;
using Microsoft.Extensions.Logging;
using Moq.Protected;
using Moq;
using System.Net;

namespace Test.Api.ACCAI.Infrastructure.Adapters.ExternalServices;

[TestFixture]
public class AccaiChangeFpServiceTests
{
    private Mock<ILogger<AccaiChangeFpService>> _loggerMock;

    [SetUp]
    public void Setup()
    {
        _loggerMock = new Mock<ILogger<AccaiChangeFpService>>();
    }

    // ✅ Helper que devuelve siempre la misma respuesta simulada
    private static HttpClient CreateHttpClient(HttpResponseMessage responseMessage)
    {
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);

        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(responseMessage);

        return new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };
    }

    [Test]
    public async Task SendChangeAsync_ShouldReturnTrue_OnSuccess()
    {
        // Arrange
        var client = CreateHttpClient(new HttpResponseMessage(HttpStatusCode.OK));
        var service = new AccaiChangeFpService(client, _loggerMock.Object);

        // Act
        var result = await service.SendChangeAsync(new ChangeFpItem { Contract = 1 });

        // Assert
        Assert.That(result, Is.True);
    }

    [Test]
    public void SendChangeAsync_ShouldThrowExternalServiceException_OnTimeout()
    {
        // Arrange
        var handler = new Mock<HttpMessageHandler>();
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new TaskCanceledException("Request timed out"));

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var service = new AccaiChangeFpService(client, _loggerMock.Object);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ExternalServiceException>(
            async () => await service.SendChangeAsync(new ChangeFpItem { Contract = 1 }));

        Assert.That(ex.Code, Is.EqualTo("http.timeout"));
    }

    [Test]
    public void SendChangeAsync_ShouldThrowExternalServiceException_OnNetworkError()
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

        var service = new AccaiChangeFpService(client, _loggerMock.Object);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ExternalServiceException>(
            async () => await service.SendChangeAsync(new ChangeFpItem { Contract = 4 }));

        Assert.That(ex.Code, Is.EqualTo("http.network"));
    }

    [Test]
    public void SendChangeAsync_ShouldThrowExternalServiceException_OnUnexpectedError()
    {
        // Arrange: simulamos un fallo no esperado lanzando excepción genérica
        var handler = new Mock<HttpMessageHandler>(MockBehavior.Strict);
        handler
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("Unexpected!"));

        var client = new HttpClient(handler.Object)
        {
            BaseAddress = new Uri("http://localhost/")
        };

        var service = new AccaiChangeFpService(client, _loggerMock.Object);

        // Act & Assert
        var ex = Assert.ThrowsAsync<ExternalServiceException>(
            async () => await service.SendChangeAsync(new ChangeFpItem { Contract = 5 }));

        Assert.That(ex.Code, Is.EqualTo("external.error"));
    }
}
