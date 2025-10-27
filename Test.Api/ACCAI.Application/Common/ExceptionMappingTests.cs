using ACCAI.Application.Common;
using System.Net.Sockets;
using System.Net;

namespace Test.Api.ACCAI.Application.Common;

[TestFixture]
public class ExceptionMappingTests
{
    [Test]
    public void ToApiError_ShouldReturnCustom_WhenOverridesProvided()
    {
        var ex = new Exception("ignored");
        var error = ex.ToApiError("target", codeOverride: "custom.code", messageOverride: "Custom message");

        Assert.That(error.Code, Is.EqualTo("custom.code"));
        Assert.That(error.Message, Is.EqualTo("Custom message"));
        Assert.That(error.Target, Is.EqualTo("target"));
    }

    [Test]
    public void ToApiError_ShouldMapExternalServiceException()
    {
        var ex = new ExternalServiceException("E001", "External failed", "extTarget");
        var error = ex.ToApiError("api");

        Assert.That(error.Code, Is.EqualTo("E001"));
        Assert.That(error.Message, Is.EqualTo("External failed"));
        Assert.That(error.Target, Is.EqualTo("extTarget"));
    }

    [Test]
    public void ToApiError_ShouldMapRepositoryException()
    {
        var ex = new RepositoryException("R001", "Repo failed", "repoTarget");
        var error = ex.ToApiError("api");

        Assert.That(error.Code, Is.EqualTo("R001"));
        Assert.That(error.Message, Is.EqualTo("Repo failed"));
        Assert.That(error.Target, Is.EqualTo("repoTarget"));
    }

    [Test]
    public void ToApiError_ShouldMapTaskCanceledException()
    {
        var ex = new TaskCanceledException();
        var error = ex.ToApiError("api");

        Assert.That(error.Code, Is.EqualTo("http.timeout"));
        Assert.That(error.Message, Does.Contain("Tiempo de espera agotado"));
        Assert.That(error.Target, Is.EqualTo("api"));
    }

    [Test]
    public void ToApiError_ShouldMapDnsUnresolved_WhenSocketError()
    {
        var socketEx = new SocketException((int)SocketError.HostNotFound);
        var httpEx = new HttpRequestException("DNS fail", socketEx);
        var error = httpEx.ToApiError("api");

        Assert.That(error.Code, Is.EqualTo("http.dns_unresolved"));
        Assert.That(error.Message, Does.Contain("No se pudo resolver"));
        Assert.That(error.Target, Is.EqualTo("api"));
    }

    [Test]
    public void ToApiError_ShouldMapHttpStatusCode_WhenProvided()
    {
        var httpEx = new HttpRequestException("Service down", null, HttpStatusCode.BadGateway);
        var error = httpEx.ToApiError("api");

        Assert.That(error.Code, Is.EqualTo("http.502"));
        Assert.That(error.Message, Does.Contain("Error al llamar"));
        Assert.That(error.Target, Is.EqualTo("api"));
    }

    [Test]
    public void ToApiError_ShouldMapUnknown_WhenUnhandledException()
    {
        var ex = new InvalidOperationException("boom");
        var error = ex.ToApiError("api");

        Assert.That(error.Code, Is.EqualTo("unknown"));
        Assert.That(error.Message, Is.EqualTo("Error inesperado."));
        Assert.That(error.Target, Is.EqualTo("api"));
    }

    [Test]
    public void ApiResult_Success_ShouldSetOkAndData()
    {
        var result = ApiResult<string>.Success("cid123", "hello");

        Assert.That(result.CorrelationId, Is.EqualTo("cid123"));
        Assert.That(result.Ok, Is.True);
        Assert.That(result.Data, Is.EqualTo("hello"));
        Assert.That(result.Errors, Is.Empty);
    }

    [Test]
    public void ApiResult_Fail_ShouldSetErrors()
    {
        var errors = new[] { new ApiError("E1", "Failed") };
        var result = ApiResult<string>.Fail("cid456", errors);

        Assert.That(result.CorrelationId, Is.EqualTo("cid456"));
        Assert.That(result.Ok, Is.False);
        Assert.That(result.Data, Is.Null);
        Assert.That(result.Errors.Count, Is.EqualTo(1));
    }

    [Test]
    public void ApiError_ShouldAssignAllProperties()
    {
        var err = new ApiError("C1", "Message", "Target", 10, "Field");

        Assert.That(err.Code, Is.EqualTo("C1"));
        Assert.That(err.Message, Is.EqualTo("Message"));
        Assert.That(err.Target, Is.EqualTo("Target"));
        Assert.That(err.Line, Is.EqualTo(10));
        Assert.That(err.Field, Is.EqualTo("Field"));
    }

    [Test]
    public void ExternalServiceException_ShouldSetProperties()
    {
        var inner = new Exception("inner");
        var ex = new ExternalServiceException("E1", "Fail", "tgt", 500, inner);

        Assert.That(ex.Code, Is.EqualTo("E1"));
        Assert.That(ex.Message, Is.EqualTo("Fail"));
        Assert.That(ex.Target, Is.EqualTo("tgt"));
        Assert.That(ex.StatusCode, Is.EqualTo(500));
        Assert.That(ex.InnerException, Is.EqualTo(inner));
    }

    [Test]
    public void RepositoryException_ShouldSetProperties()
    {
        var inner = new Exception("inner");
        var ex = new RepositoryException("R1", "Fail", "repo", inner);

        Assert.That(ex.Code, Is.EqualTo("R1"));
        Assert.That(ex.Message, Is.EqualTo("Fail"));
        Assert.That(ex.Target, Is.EqualTo("repo"));
        Assert.That(ex.InnerException, Is.EqualTo(inner));
    }
}