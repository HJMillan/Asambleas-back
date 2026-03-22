using Asambleas.Common.Exceptions;
using Asambleas.Infrastructure.Middleware;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Asambleas.Tests.Infrastructure;

/// <summary>
/// Tests para el GlobalExceptionHandler (H34).
/// Verifica que cada tipo de excepción mapea al status code HTTP correcto.
/// </summary>
public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _sut =
        new(Substitute.For<ILogger<GlobalExceptionHandler>>());

    [Fact]
    public async Task UnauthorizedException_Returns401()
    {
        var ctx = CreateHttpContext();
        var handled = await _sut.TryHandleAsync(ctx, new UnauthorizedAccessException("no auth"), CancellationToken.None);

        handled.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status401Unauthorized);
    }

    [Fact]
    public async Task DuplicateEntityException_Returns409()
    {
        var ctx = CreateHttpContext();
        var handled = await _sut.TryHandleAsync(ctx, new DuplicateEntityException("duplicado"), CancellationToken.None);

        handled.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status409Conflict);
    }

    [Fact]
    public async Task EntityNotFoundException_Returns404()
    {
        var ctx = CreateHttpContext();
        var handled = await _sut.TryHandleAsync(ctx, new EntityNotFoundException("User", Guid.Empty), CancellationToken.None);

        handled.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status404NotFound);
    }

    [Fact]
    public async Task BusinessRuleException_Returns422()
    {
        var ctx = CreateHttpContext();
        var handled = await _sut.TryHandleAsync(ctx, new BusinessRuleException("regla"), CancellationToken.None);

        handled.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status422UnprocessableEntity);
    }

    [Fact]
    public async Task ArgumentException_Returns400()
    {
        var ctx = CreateHttpContext();
        var handled = await _sut.TryHandleAsync(ctx, new ArgumentException("bad arg"), CancellationToken.None);

        handled.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status400BadRequest);
    }

    [Fact]
    public async Task GenericException_Returns500()
    {
        var ctx = CreateHttpContext();
        var handled = await _sut.TryHandleAsync(ctx, new Exception("boom"), CancellationToken.None);

        handled.Should().BeTrue();
        ctx.Response.StatusCode.Should().Be(StatusCodes.Status500InternalServerError);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        return ctx;
    }
}
