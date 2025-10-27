using ACCAI.Application.FpChanges;
using ACCAI.Domain.Ports.ExternalServices;
using ACCAI.Domain.Ports;
using FluentValidation;
using Microsoft.Extensions.Logging;
using Moq;
using System.Text;
using System.ComponentModel.DataAnnotations;
using ACCAI.Domain.ReadModels;
using FluentValidation.Results;

namespace Test.Api.ACCAI.Application.FpChanges;

[TestFixture]
public class ValidateFpChangesCsvCommandHandlerTests
{
    private Mock<IFpChangeCsvParser> _parser;
    private Mock<IValidator<FpChangeCsvRow>> _validator;
    private Mock<ILogger<ValidateFpChangesCsvCommandHandler>> _logger;
    private Mock<IChangeFpFactory> _factory;
    private Mock<IContractsRepository> _contractsRepo;
    private ValidateFpChangesCsvCommandHandler _handler;

    [SetUp]
    public void Setup()
    {
        _parser = new Mock<IFpChangeCsvParser>();
        _validator = new Mock<IValidator<FpChangeCsvRow>>();
        _logger = new Mock<ILogger<ValidateFpChangesCsvCommandHandler>>();
        _factory = new Mock<IChangeFpFactory>();
        _contractsRepo = new Mock<IContractsRepository>();

        _handler = new ValidateFpChangesCsvCommandHandler(
            _parser.Object,
            _validator.Object,
            _logger.Object,
            _factory.Object,
            _contractsRepo.Object);
    }

    private static MemoryStream CreateStream(string content) =>
        new MemoryStream(Encoding.UTF8.GetBytes(content));

    private static CsvParseOutput<FpChangeCsvRow> ValidCsvOutput(int rowCount = 1)
    {
        var header = new[]
        {
            "Apellidos","Nombres","NroDocum","TipoDocum","Producto","PlanProducto","Contrato",
            "Empresa","Segmento","Ciudad","IdAgte","NombreAgte","IdAgteNuevo","NombreAgteNuevo",
            "SubGrupoFp","descripcion"
        }.ToList();

        var rows = Enumerable.Range(0, rowCount)
            .Select(i => new FpChangeCsvRow("Ape", "Nom", "123", "C", "ACCAI", "Plan", "456",
                                            "Emp", "Seg", "Ciu", "1", "2", "Nuevo", "Sub", "desc"))
            .ToList();

        return new CsvParseOutput<FpChangeCsvRow>(header, rows);
    }

    [Test]
    public async Task Handle_ShouldFail_WhenExtensionNotCsv()
    {
        var cmd = new ValidateFpChangesCsvCommand(CreateStream(""), 10, "test.txt");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.That(result.Errores, Is.EqualTo(1));
        Assert.That(result.Detalle[0].Mensaje, Does.Contain("extensión"));
    }

    [Test]
    public async Task Handle_ShouldFail_WhenFileEmpty()
    {
        var cmd = new ValidateFpChangesCsvCommand(CreateStream(""), 0, "file.csv");
        var result = await _handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Detalle[0].Mensaje, Does.Contain("vacío"));
    }

    [Test]
    public async Task Handle_ShouldFail_WhenFileTooLarge()
    {
        var cmd = new ValidateFpChangesCsvCommand(CreateStream(""), 2_000_000, "file.csv");
        var result = await _handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Detalle[0].Mensaje, Does.Contain("1MB"));
    }

    [Test]
    public async Task Handle_ShouldFail_WhenHeaderInvalid()
    {
        _parser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new CsvParseOutput<FpChangeCsvRow>(
                new List<string> { "BadHeader" },
                new List<FpChangeCsvRow>()));

        var cmd = new ValidateFpChangesCsvCommand(CreateStream("abc"), 100, "ok.csv");
        var result = await _handler.Handle(cmd, CancellationToken.None);
        Assert.That(result.Detalle[0].Mensaje, Does.Contain("Cabeceras inválidas"));
    }

    [Test]
    public async Task Handle_ShouldFail_WhenRowsExceedLimit()
    {
        var parsed = ValidCsvOutput(51);
        _parser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parsed);

        var cmd = new ValidateFpChangesCsvCommand(CreateStream("data"), 100, "ok.csv");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.That(result.Detalle[0].Mensaje, Does.Contain("50"));
    }

    [Test]
    public async Task Handle_ShouldAddValidationErrors_FromValidator()
    {
        var parsed = ValidCsvOutput(1);
        _parser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parsed);

        _validator.Setup(v => v.ValidateAsync(It.IsAny<FpChangeCsvRow>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult(new[]
            {
                new ValidationFailure(nameof(FpChangeCsvRow.Producto), "Producto inválido")
            }));

        _factory.Setup(f => f.GetService(It.IsAny<string>()))
            .Returns(Mock.Of<IChangeFpService>());

        var cmd = new ValidateFpChangesCsvCommand(CreateStream("data"), 100, "ok.csv");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.That(result.Errores, Is.EqualTo(1));
        Assert.That(result.Detalle[0].Campo, Is.EqualTo(nameof(FpChangeCsvRow.Producto)));
    }

    [Test]
    public async Task Handle_ShouldSkipRows_WithDisallowedProduct()
    {
        var row = new FpChangeCsvRow("Ape", "Nom", "123", "C", "XYZ", "Plan", "456", "Emp", "Seg",
                                     "Ciu", "1", "2", "Nuevo", "Sub", "desc");

        var parsed = new CsvParseOutput<FpChangeCsvRow>(
            ValidCsvOutput().Header,
            new List<FpChangeCsvRow> { row });

        _parser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parsed);

        _validator.Setup(v => v.ValidateAsync(It.IsAny<FpChangeCsvRow>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _factory.Setup(f => f.GetService(It.IsAny<string>()))
            .Returns(Mock.Of<IChangeFpService>());

        var cmd = new ValidateFpChangesCsvCommand(CreateStream("data"), 100, "ok.csv");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.That(result.TotalFilas, Is.EqualTo(1));
        Assert.That(result.Errores, Is.EqualTo(0));
    }

    [Test]
    public async Task Handle_ShouldAddError_WhenServiceThrowsException()
    {
        var parsed = ValidCsvOutput(1);
        _parser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parsed);

        var mockService = new Mock<IChangeFpService>();
        mockService.Setup(s => s.SendChangeAsync(It.IsAny<ChangeFpItem>()))
            .ThrowsAsync(new Exception("external fail"));

        _factory.Setup(f => f.GetService("ACCAI")).Returns(mockService.Object);

        _validator.Setup(v => v.ValidateAsync(It.IsAny<FpChangeCsvRow>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _contractsRepo.Setup(r => r.UpdateContractsAgentAsync(It.IsAny<List<ChangeFpItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var cmd = new ValidateFpChangesCsvCommand(CreateStream("data"), 100, "ok.csv");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.That(result.Errores, Is.GreaterThan(0));
        Assert.That(result.Detalle[0].Mensaje, Does.Contain("unknown: Error inesperado."));
    }

    [Test]
    public async Task Handle_ShouldAddError_WhenRepositoryFails()
    {
        var parsed = ValidCsvOutput(1);
        _parser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parsed);

        var mockService = new Mock<IChangeFpService>();
        mockService.Setup(s => s.SendChangeAsync(It.IsAny<ChangeFpItem>()))
            .ReturnsAsync(true);

        _factory.Setup(f => f.GetService("ACCAI")).Returns(mockService.Object);

        _validator.Setup(v => v.ValidateAsync(It.IsAny<FpChangeCsvRow>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _contractsRepo.Setup(r => r.UpdateContractsAgentAsync(It.IsAny<List<ChangeFpItem>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new Exception("db fail"));

        var cmd = new ValidateFpChangesCsvCommand(CreateStream("data"), 100, "ok.csv");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.That(result.Errores, Is.GreaterThan(0));
        Assert.That(result.Detalle[0].Campo, Is.EqualTo("_db"));
    }

    [Test]
    public async Task Handle_ShouldSucceed_WhenAllValid()
    {
        var parsed = ValidCsvOutput(1);
        _parser.Setup(x => x.ParseAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(parsed);

        _validator.Setup(v => v.ValidateAsync(It.IsAny<FpChangeCsvRow>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        var mockService = new Mock<IChangeFpService>();
        mockService.Setup(s => s.SendChangeAsync(It.IsAny<ChangeFpItem>()))
            .ReturnsAsync(true);

        _factory.Setup(f => f.GetService("ACCAI")).Returns(mockService.Object);

        _contractsRepo.Setup(r => r.UpdateContractsAgentAsync(It.IsAny<List<ChangeFpItem>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var cmd = new ValidateFpChangesCsvCommand(CreateStream("data"), 100, "ok.csv");
        var result = await _handler.Handle(cmd, CancellationToken.None);

        Assert.That(result.TotalFilas, Is.EqualTo(1));
        Assert.That(result.Errores, Is.EqualTo(0));
    }
}
