using System.Security.Cryptography;
using System.Text;
using Asambleas.Common;
using Asambleas.Features.Auth;
using Asambleas.Features.Auth.Entities;
using Asambleas.Infrastructure.Database;
using Asambleas.Infrastructure.Security;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Asambleas.Tests.Features.Auth;

/// <summary>
/// Tests de integración para AuthService usando InMemory DB.
/// Adaptado para Result&lt;T&gt; pattern y IUnitOfWork.
/// </summary>
public class AuthServiceTests : IDisposable
{
    private readonly ApplicationDbContext _db;
    private readonly AuthService _sut;

    public AuthServiceTests()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .ConfigureWarnings(w => w.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;
        _db = new ApplicationDbContext(options);

        var jwtSettings = Options.Create(new JwtSettings
        {
            Secret = "TestSecret_MustBeAtLeast32CharactersLong!!",
            Issuer = "Test",
            Audience = "Test",
            AccessTokenExpirationMinutes = 15,
            RefreshTokenExpirationDays = 7
        });
        var jwt = new JwtTokenService(jwtSettings);
        var ldap = Substitute.For<ILdapService>();
        var domelec = Substitute.For<IDomelecService>();
        domelec.VerificarAsync(Arg.Any<string>()).Returns(true);
        var logger = Substitute.For<ILogger<AuthService>>();

        var userRepo = new UserRepository(_db);
        var tokenRepo = new RefreshTokenRepository(_db);

        // ApplicationDbContext implementa IUnitOfWork, asi que se puede pasar directamente
        _sut = new AuthService(userRepo, tokenRepo, _db, jwt, jwtSettings, ldap, domelec, logger);
    }

    private RegisterRequest ValidRegister(string cuil = "20345678901", string email = "a@b.com")
        => new(cuil, "Juan", "Pérez", email, "Password1!", "Password1!");

    [Fact]
    public async Task Register_ValidRequest_CreatesUser()
    {
        var result = await _sut.RegisterAsync(ValidRegister());

        result.IsSuccess.Should().BeTrue();
        var (user, access, refresh) = result.Value!;
        user.Should().NotBeNull();
        user.Cuil.Should().Be("20345678901");
        access.Should().NotBeNullOrEmpty();
        refresh.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Register_DuplicateCuil_ReturnsFailure()
    {
        await _sut.RegisterAsync(ValidRegister());
        var result = await _sut.RegisterAsync(ValidRegister(email: "other@b.com"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("CUIL");
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsFailure()
    {
        await _sut.RegisterAsync(ValidRegister());
        var result = await _sut.RegisterAsync(ValidRegister(cuil: "27345678902"));

        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("email");
    }

    [Fact]
    public async Task LoginCuil_WrongPassword_IncrementsAttempts()
    {
        await _sut.RegisterAsync(ValidRegister());
        var result = await _sut.LoginCuilAsync(new LoginCuilRequest("20345678901", "WrongPass1!"));

        result.IsFailure.Should().BeTrue();
        var user = await _db.Users.FirstAsync(u => u.Cuil == "20345678901");
        user.LoginAttempts.Should().Be(1);
    }

    [Fact]
    public async Task LoginCuil_5Failures_LocksAccount()
    {
        await _sut.RegisterAsync(ValidRegister());

        for (int i = 0; i < 5; i++)
        {
            await _sut.LoginCuilAsync(new LoginCuilRequest("20345678901", "Wrong1!"));
        }

        var user = await _db.Users.FirstAsync(u => u.Cuil == "20345678901");
        user.LockoutEnd.Should().NotBeNull();
        user.LockoutEnd.Should().BeAfter(DateTime.UtcNow);
        user.ConsecutiveLockouts.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task LoginCuil_CorrectPassword_ResetsAttempts()
    {
        await _sut.RegisterAsync(ValidRegister());

        // Fail once
        await _sut.LoginCuilAsync(new LoginCuilRequest("20345678901", "Wrong1!"));

        // Succeed
        var result = await _sut.LoginCuilAsync(new LoginCuilRequest("20345678901", "Password1!"));
        result.IsSuccess.Should().BeTrue();
        result.Value!.User.LoginAttempts.Should().Be(0);
        result.Value!.User.ConsecutiveLockouts.Should().Be(0);
    }

    [Fact]
    [Trait("Category", "RequiresDb")]
    public async Task RevokeAll_WithActiveTokens_RequiresRealDb()
    {
        // ExecuteUpdateAsync no es soportado por InMemory provider.
        // Este test documenta la limitación. La lógica completa
        // requiere tests de integración con PostgreSQL real (Testcontainers).
        // Marcado con [Trait("Category", "RequiresDb")] para filtrar en CI.
        var result = await _sut.RegisterAsync(ValidRegister());
        result.IsSuccess.Should().BeTrue();

        // InMemory no soporta ExecuteUpdateAsync
        var act = async () => await _sut.RevokeAllTokensAsync(result.Value!.User.Id);
        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task GetById_ExistingUser_ReturnsUser()
    {
        var result = await _sut.RegisterAsync(ValidRegister());
        result.IsSuccess.Should().BeTrue();

        var found = await _sut.GetByIdAsync(result.Value!.User.Id);

        found.Should().NotBeNull();
        found!.Cuil.Should().Be("20345678901");
    }

    [Fact]
    public async Task GetById_NonExistent_ReturnsNull()
    {
        var result = await _sut.GetByIdAsync(Guid.NewGuid());
        result.Should().BeNull();
    }

    // ── Tests nuevos: Logout y Refresh con token revocado (#9, #10) ──

    [Fact]
    public async Task Logout_WithRefreshToken_RevokesToken()
    {
        var result = await _sut.RegisterAsync(ValidRegister());
        result.IsSuccess.Should().BeTrue();
        var (user, _, refreshToken) = result.Value!;

        await _sut.LogoutAsync(user.Id, refreshToken);

        // Los refresh tokens ahora se almacenan hasheados (SHA-256)
        var hashedToken = Convert.ToBase64String(
            SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken)));
        var tokens = await _db.RefreshTokens
            .Where(t => t.UserId == user.Id && t.Token == hashedToken)
            .ToListAsync();

        tokens.Should().ContainSingle();
        tokens[0].IsRevoked.Should().BeTrue();
        tokens[0].RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task RefreshToken_WithRevokedToken_DetectsReuse()
    {
        // Register y obtener tokens
        var result = await _sut.RegisterAsync(ValidRegister());
        result.IsSuccess.Should().BeTrue();
        var (user, accessToken, refreshToken) = result.Value!;

        // Logout (revoca el refresh token)
        await _sut.LogoutAsync(user.Id, refreshToken);

        // Intentar refresh con el token revocado → debe detectar reuso
        var refreshResult = await _sut.RefreshTokenAsync(accessToken, refreshToken);
        refreshResult.IsFailure.Should().BeTrue();
        refreshResult.Error.Should().Contain("comprometida");
    }

    [Fact]
    public async Task Logout_WithNullRefreshToken_DoesNotThrow()
    {
        var result = await _sut.RegisterAsync(ValidRegister());
        result.IsSuccess.Should().BeTrue();

        var act = async () => await _sut.LogoutAsync(result.Value!.User.Id, null);
        await act.Should().NotThrowAsync();
    }

    public void Dispose() => _db.Dispose();
}
