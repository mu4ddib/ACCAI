using System.Text;
using ACCAI.Api.Endpoints;
using ACCAI.Application.Dtos;
using ACCAI.Application.FpChanges;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Test.Api.ACCAI.Api.Controllers;

[TestFixture]
public class FpChangesEndpointsTests
{
    private Mock<IMediator> _mediatorMock = null!;
    private WebApplicationBuilder _builder = null!;
    private WebApplication _app = null!;

    [SetUp]
    public void Setup()
    {
        _mediatorMock = new Mock<IMediator>();
        _builder = WebApplication.CreateBuilder();
        _builder.Services.AddSingleton(_mediatorMock.Object);
        _app = _builder.Build();
    }
    [TearDown]
    public void TearDown()
    {
        _app.DisposeAsync();
        _builder = null!;
        _mediatorMock = null!;
    }

    [Test]
    public async Task Upload_ShouldReturnBadRequest_WhenFileIsNull()
    {
        // Arrange
        IFormFile file = null!;
        var ct = CancellationToken.None;

        // Act
        var result = await InvokeUploadEndpoint(file, ct);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequest<ValidationResponseDto>>());
        var bad = (BadRequest<ValidationResponseDto>)result;
        Assert.That(bad.Value!.Errores, Is.EqualTo(1));
        Assert.That(bad.Value.Detalle.Count, Is.EqualTo(1));
    }

    [Test]
    public async Task Upload_ShouldReturnBadRequest_WhenFileIsEmpty()
    {
        // Arrange
        var emptyFile = new FormFile(new MemoryStream(), 0, 0, "file", "empty.csv");
        var ct = CancellationToken.None;

        // Act
        var result = await InvokeUploadEndpoint(emptyFile, ct);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequest<ValidationResponseDto>>());
        var bad = (BadRequest<ValidationResponseDto>)result;
        Assert.That(bad.Value!.Errores, Is.EqualTo(1));
        Assert.That(bad.Value.Detalle[0].Campo, Is.EqualTo("_archivo"));
    }

    [Test]
    public async Task Upload_ShouldReturnOk_WhenNoErrors()
    {
        // Arrange
        var content = "col1,col2";
        var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
        var file = new FormFile(stream, 0, stream.Length, "file", "ok.csv");

        var expected = new ValidationResponseDto(5, 0, "cid123", new List<RowError>());
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ValidateFpChangesCsvCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        // Act
        var result = await InvokeUploadEndpoint(file, CancellationToken.None);

        // Assert
        Assert.That(result, Is.TypeOf<Ok<ValidationResponseDto>>());
        var ok = (Ok<ValidationResponseDto>)result;
        Assert.That(ok.Value!.TotalFilas, Is.EqualTo(5));
        Assert.That(ok.Value.Errores, Is.EqualTo(0));
    }

    [Test]
    public async Task Upload_ShouldReturnBadRequest_WhenValidationHasErrors()
    {
        // Arrange
        var stream = new MemoryStream(Encoding.UTF8.GetBytes("invalid"));
        var file = new FormFile(stream, 0, stream.Length, "file", "fail.csv");

        var response = new ValidationResponseDto(3, 2, "cid", new List<RowError>
        {
            new RowError(1, "Campo", "Error", "Valor")
        });
        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ValidateFpChangesCsvCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(response);

        // Act
        var result = await InvokeUploadEndpoint(file, CancellationToken.None);

        // Assert
        Assert.That(result, Is.TypeOf<BadRequest<ValidationResponseDto>>());
        var bad = (BadRequest<ValidationResponseDto>)result;
        Assert.That(bad.Value!.Errores, Is.EqualTo(2));
    }

    // 🔹 Helper simulating endpoint delegate
    private async Task<IResult> InvokeUploadEndpoint(IFormFile? file, CancellationToken ct)
    {
        if (file is null || file.Length == 0)
        {
            var cid = Guid.NewGuid().ToString("N");
            return Results.BadRequest(ValidationResponseDto.Fail("Archivo vacío.", cid));
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        ms.Position = 0;

        var res = await _mediatorMock.Object.Send(
            new ValidateFpChangesCsvCommand(ms, file.Length, file.FileName), ct);

        return res.Errores == 0 ? Results.Ok(res) : Results.BadRequest(res);
    }


    [Test]
    public async Task Should_Return_BadRequest_When_File_Is_Null()
    {
        // Arrange
        var group = _app.MapGroup("/api/fp-changes");
        FpChangesEndpoints.Map(_app);

        IFormFile? file = null;
        var mediator = _mediatorMock.Object;
        var ct = CancellationToken.None;

        // Act
        var result = await InvokeUploadEndpoint(file, mediator, ct);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequest<ValidationResponseDto>>());
        var bad = (BadRequest<ValidationResponseDto>)result!;
        Assert.That(bad.Value.Detalle.First().Mensaje, Is.EqualTo("Archivo vacío."));
    }

    [Test]
    public async Task Should_Return_BadRequest_When_File_Is_Empty()
    {
        // Arrange
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.Length).Returns(0);
        fileMock.Setup(f => f.FileName).Returns("empty.csv");

        var result = await InvokeUploadEndpoint(fileMock.Object, _mediatorMock.Object, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequest<ValidationResponseDto>>());
        var bad = (BadRequest<ValidationResponseDto>)result!;
        Assert.That(bad.Value.Detalle.First().Mensaje, Is.EqualTo("Archivo vacío."));
    }

    [Test]
    public async Task Should_Return_Ok_When_Validation_Has_No_Errors()
    {
        // Arrange
        var csvBytes = Encoding.UTF8.GetBytes("header1,header2\nvalue1,value2");
        var fileMock = CreateMockFormFile(csvBytes, "data.csv");
        var dto = new ValidationResponseDto(2, 0, "cid123", new List<RowError>());

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ValidateFpChangesCsvCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await InvokeUploadEndpoint(fileMock.Object, _mediatorMock.Object, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<Ok<ValidationResponseDto>>());
        var ok = (Ok<ValidationResponseDto>)result!;
        Assert.That(ok.Value.Errores, Is.EqualTo(0));
        Assert.That(ok.Value.TotalFilas, Is.EqualTo(2));
    }

    [Test]
    public async Task Should_Return_BadRequest_When_Validation_Has_Errors()
    {
        // Arrange
        var csvBytes = Encoding.UTF8.GetBytes("header1,header2\nvalue1,value2");
        var fileMock = CreateMockFormFile(csvBytes, "invalid.csv");
        var dto = new ValidationResponseDto(2, 3, "cid123", new List<RowError>
            {
                new(1, "Campo1", "Error", "X")
            });

        _mediatorMock
            .Setup(m => m.Send(It.IsAny<ValidateFpChangesCsvCommand>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(dto);

        // Act
        var result = await InvokeUploadEndpoint(fileMock.Object, _mediatorMock.Object, CancellationToken.None);

        // Assert
        Assert.That(result, Is.InstanceOf<BadRequest<ValidationResponseDto>>());
        var bad = (BadRequest<ValidationResponseDto>)result!;
        Assert.That(bad.Value.Errores, Is.EqualTo(3));
    }

    private static Mock<IFormFile> CreateMockFormFile(byte[] content, string name)
    {
        var stream = new MemoryStream(content);
        var fileMock = new Mock<IFormFile>();
        fileMock.Setup(f => f.OpenReadStream()).Returns(stream);
        fileMock.Setup(f => f.FileName).Returns(name);
        fileMock.Setup(f => f.Length).Returns(content.Length);
        fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                .Callback<Stream, CancellationToken>((s, _) => stream.CopyTo(s))
                .Returns(Task.CompletedTask);
        return fileMock;
    }

    private static async Task<IResult> InvokeUploadEndpoint(IFormFile? file, IMediator mediator, CancellationToken ct)
    {
        // Simula la lógica inline del MapPost (sin arrancar servidor)
        if (file is null || file.Length == 0)
        {
            var cid = Guid.NewGuid().ToString("N");
            return Results.BadRequest(ValidationResponseDto.Fail("Archivo vacío.", cid));
        }

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms, ct);
        ms.Position = 0;
        var res = await mediator.Send(new ValidateFpChangesCsvCommand(ms, file.Length, file.FileName), ct);
        return res.Errores == 0 ? Results.Ok(res) : Results.BadRequest(res);
    }
}
