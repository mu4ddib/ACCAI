using MediatR;
using Microsoft.EntityFrameworkCore;
using ACCAI.Application.Abstractions;
using ACCAI.Application.Voters;
using ACCAI.Infrastructure.DataSource;
using ACCAI.Infrastructure.Extensions;
using ACCAI.Domain.Services;
using Prometheus;
using FluentValidation;
using Serilog;


Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/accai-validation-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog();

var cfg = builder.Configuration;
builder.Services.AddDbContext<DataContext>(opt => opt.UseSqlServer(cfg.GetConnectionString("db")));
builder.Services.AddMediatR(cfgr => cfgr.RegisterServicesFromAssembly(typeof(ApplicationAssembly).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(ApplicationAssembly).Assembly);
builder.Services.AddHealthChecks().AddDbContextCheck<DataContext>();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAttributedServices(typeof(RecordVoterService).Assembly, typeof(DataContext).Assembly);
builder.Services.AddInfrastructureServices(cfg);
builder.Services.AddInfrastructure();
var app = builder.Build();
//if (app.Environment.IsDevelopment()){ app.UseSwagger(); app.UseSwaggerUI(); }
app.UseSwagger(); 
app.UseSwaggerUI();
app.UseRouting();
app.UseHttpMetrics();
app.UseSerilogRequestLogging();
app.UseMiddleware<ACCAI.Api.Middleware.ExceptionMiddleware>();
ACCAI.Api.Endpoints.VoterEndpoints.Map(app);
ACCAI.Api.Endpoints.FpChangesEndpoints.Map(app);
app.MapHealthChecks("/health");
app.MapMetrics();
app.Run();
