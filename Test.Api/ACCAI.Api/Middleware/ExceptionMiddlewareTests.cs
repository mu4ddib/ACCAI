using ACCAI.Api.Middleware;
using ACCAI.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using System.Net;
using System.Text.Json;

namespace Test.Api.ACCAI.Api.Middleware;

[TestFixture]
public class ExceptionMiddlewareTests
{
    private Mock<ILogger<ExceptionMiddleware>> _loggerMock = null!;
    private DefaultHttpContext _context = null!;

    [SetUp]
    public void SetUp()
    {
        _loggerMock = new Mock<ILogger<ExceptionMiddleware>>();
        _context = new DefaultHttpContext();
        _context.Response.Body = new MemoryStream(); // necesario para capturar el JSON
    }

    [Test]
    public async Task Should_InvokeNext_When_NoException()
    {
        // Arrange
        var called = false;
        RequestDelegate next = ctx =>
        {
            called = true;
            return Task.CompletedTask;
        };
        var middleware = new ExceptionMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.That(called, Is.True);
        Assert.That(_context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.OK));
        _loggerMock.VerifyNoOtherCalls();
    }

    [Test]
    public async Task Should_ReturnBadRequest_When_CoreBusinessException()
    {
        // Arrange
        RequestDelegate next = ctx => throw new CoreBusinessException("Business rule violated");
        var middleware = new ExceptionMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.That(_context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.BadRequest));

        _context.Response.Body.Position = 0;
        var json = await new StreamReader(_context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(json);

        Assert.That(response.GetProperty("error").GetString(), Is.EqualTo("Business rule violated"));

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<CoreBusinessException>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }

    [Test]
    public async Task Should_ReturnInternalServerError_When_UnhandledException()
    {
        // Arrange
        RequestDelegate next = ctx => throw new InvalidOperationException("boom!");
        var middleware = new ExceptionMiddleware(next, _loggerMock.Object);

        // Act
        await middleware.InvokeAsync(_context);

        // Assert
        Assert.That(_context.Response.StatusCode, Is.EqualTo((int)HttpStatusCode.InternalServerError));

        _context.Response.Body.Position = 0;
        var json = await new StreamReader(_context.Response.Body).ReadToEndAsync();
        var response = JsonSerializer.Deserialize<JsonElement>(json);

        Assert.That(response.GetProperty("error").GetString(), Is.EqualTo("Unexpected error"));
        Assert.That(response.GetProperty("traceId").GetString(), Is.Not.Null);

        _loggerMock.Verify(
            l => l.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception>(),
                (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
            Times.Once);
    }
}