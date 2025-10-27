using ACCAI.Api;
using ACCAI.Application.Dtos;
using Microsoft.AspNetCore.Mvc.Testing;
using System.Net.Http.Json;
using System.Net;
using Microsoft.EntityFrameworkCore;
using ACCAI.Infrastructure.DataSource;
using Microsoft.Extensions.DependencyInjection;




namespace Test.Api.ACCAI.Api;

[TestFixture]
public class ProgramTests
{
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContext registration
                    var descriptor = services.SingleOrDefault(
                        d => d.ServiceType == typeof(DbContextOptions<DataContext>));

                    if (descriptor != null)
                        services.Remove(descriptor);

                    // Replace with InMemory
                    services.AddDbContext<DataContext>(opt =>
                        opt.UseInMemoryDatabase("TestDb"));
                });
            });

        _client = _factory.CreateClient();
    }

    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    [Test]
    public async Task Swagger_Endpoint_Should_Be_Available()
    {
        var response = await _client.GetAsync("/swagger/index.html");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var html = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("Swagger", html);
    }


    [Test]
    public async Task Metrics_Endpoint_Should_Work()
    {
        var response = await _client.GetAsync("/metrics");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var content = await response.Content.ReadAsStringAsync();
        StringAssert.Contains("http_requests_received_total", content);
    }

    [Test]
    public async Task Upload_Empty_File_Should_Return_BadRequest()
    {
        var content = new MultipartFormDataContent();
        content.Add(new ByteArrayContent(Array.Empty<byte>()), "file", "empty.csv");

        var response = await _client.PostAsync("/api/fp-changes/upload", content);

        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

        var body = await response.Content.ReadFromJsonAsync<ValidationResponseDto>();
        Assert.IsNotNull(body);
        Assert.That(body!.Errores, Is.EqualTo(1));
        Assert.That(body.Detalle.Count, Is.EqualTo(1));
        StringAssert.Contains("Archivo vacío", body.Detalle[0].Mensaje);
    }

    [Test]
    public async Task ExceptionMiddleware_Should_Handle_Unhandled_Exception()
    {
        var response = await _client.GetAsync("/nonexistent");
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
    }
}