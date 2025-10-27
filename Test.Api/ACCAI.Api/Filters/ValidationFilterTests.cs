using ACCAI.Api.Filters;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using NUnit.Framework;

namespace Test.Api.ACCAI.Api.Filters
{
    [TestFixture]
    public class ValidationFilterTests
    {
        private ValidationFilter<TestRequest> _filter = null!;
        private Mock<IValidator<TestRequest>> _validatorMock = null!;
        private DefaultHttpContext _httpContext = null!;
        private EndpointFilterDelegate _next = null!;

        [SetUp]
        public void SetUp()
        {
            _validatorMock = new Mock<IValidator<TestRequest>>();
            _filter = new ValidationFilter<TestRequest>();
            _httpContext = new DefaultHttpContext();
            _next = ctx => ValueTask.FromResult<object?>(Results.Ok("next executed"));
        }

        [Test]
        public async Task Should_CallNext_When_NoValidatorRegistered()
        {
            var services = new ServiceCollection().BuildServiceProvider();
            _httpContext.RequestServices = services;

            var req = new TestRequest { Name = "any" };
            var args = new object[] { req };
            var context = new DefaultEndpointFilterInvocationContext(_httpContext, args);

            var result = await _filter.InvokeAsync(context, _next);

            Assert.That(result, Is.TypeOf<Ok<string>>());
            Assert.That(((Ok<string>)result!).Value, Is.EqualTo("next executed"));
        }

        [Test]
        public async Task Should_ReturnBadRequest_WhenArgumentIsNull()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddSingleton<IValidator<TestRequest>>(_validatorMock.Object)
                .BuildServiceProvider();
            _httpContext.RequestServices = services;

            var args = new object?[] { null };
            var context = new DefaultEndpointFilterInvocationContext(_httpContext, args);

            // Act
            var result = await _filter.InvokeAsync(context, _next);

            // Assert
            Assert.That(result, Is.InstanceOf<IStatusCodeHttpResult>());

            var bad = (IStatusCodeHttpResult)result!;
            Assert.That(bad.StatusCode, Is.EqualTo(StatusCodes.Status400BadRequest));

            // Validamos el valor anónimo dinámicamente
            var valueProp = result!.GetType().GetProperty("Value");
            var value = valueProp!.GetValue(result);
            var errorProp = value!.GetType().GetProperty("error");

            Assert.That(errorProp, Is.Not.Null);
            Assert.That(errorProp!.GetValue(value), Is.EqualTo("Invalid request payload."));
        }


        [Test]
        public async Task Should_ReturnValidationProblem_WhenValidationFails()
        {
            // Arrange
            var services = new ServiceCollection()
                .AddSingleton<IValidator<TestRequest>>(_validatorMock.Object)
                .BuildServiceProvider();
            _httpContext.RequestServices = services;

            var req = new TestRequest { Name = "Invalid" };
            var args = new object[] { req };
            var context = new DefaultEndpointFilterInvocationContext(_httpContext, args);

            var failures = new List<ValidationFailure>
            {
                new(nameof(TestRequest.Name), "Name is required"),
                new(nameof(TestRequest.Name), "Name too short")
            };

            _validatorMock
                .Setup(v => v.ValidateAsync(req, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult(failures));

            // Act
            var result = await _filter.InvokeAsync(context, _next);

            // Assert
            // En .NET 8 devuelve ProblemHttpResult
            Assert.That(result, Is.TypeOf<ProblemHttpResult>());

            var problem = (ProblemHttpResult)result!;
            var details = problem.ProblemDetails as HttpValidationProblemDetails;

            Assert.That(details, Is.Not.Null, "Expected HttpValidationProblemDetails in ProblemHttpResult.Value");
            Assert.That(details!.Errors.ContainsKey(nameof(TestRequest.Name)), Is.True);
            Assert.That(details.Errors[nameof(TestRequest.Name)].Length, Is.EqualTo(2));
            Assert.That(details.Errors[nameof(TestRequest.Name)], Does.Contain("Name is required"));
        }


        [Test]
        public async Task Should_CallNext_WhenValidationSucceeds()
        {
            var services = new ServiceCollection()
                .AddSingleton<IValidator<TestRequest>>(_validatorMock.Object)
                .BuildServiceProvider();
            _httpContext.RequestServices = services;

            var req = new TestRequest { Name = "Valid" };
            var args = new object[] { req };
            var context = new DefaultEndpointFilterInvocationContext(_httpContext, args);

            _validatorMock
                .Setup(v => v.ValidateAsync(req, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ValidationResult());

            var result = await _filter.InvokeAsync(context, _next);

            Assert.That(result, Is.TypeOf<Ok<string>>());
            Assert.That(((Ok<string>)result!).Value, Is.EqualTo("next executed"));
        }

        public class TestRequest
        {
            public string? Name { get; set; }
        }

        private sealed class DefaultEndpointFilterInvocationContext : EndpointFilterInvocationContext
        {
            private readonly HttpContext _httpContext;
            private readonly IList<object?> _args;

            public DefaultEndpointFilterInvocationContext(HttpContext httpContext, IList<object?> args)
            {
                _httpContext = httpContext;
                _args = args;
            }

            public override HttpContext HttpContext => _httpContext;
            public override IList<object?> Arguments => _args;
            public override T GetArgument<T>(int index) => (T)_args[index]!;
        }
    }
}
