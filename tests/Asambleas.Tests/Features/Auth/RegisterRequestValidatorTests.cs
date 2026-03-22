using Asambleas.Features.Auth;
using Asambleas.Features.Auth.Validators;
using FluentAssertions;

namespace Asambleas.Tests.Features.Auth;

/// <summary>
/// Tests de validación para RegisterRequest.
/// </summary>
public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _sut = new();

    [Fact]
    public void CuilValido_11Digitos_NoDebeReportarError()
    {
        var request = new RegisterRequest("20345678901", "Juan", "Pérez", "juan@test.com", "Password123!", "Password123!");
        var result = _sut.Validate(request);
        result.Errors.Should().NotContain(e => e.PropertyName == "Cuil");
    }

    [Theory]
    [InlineData("")]
    [InlineData("123")]
    [InlineData("12345678901234")]
    [InlineData("abcdefghijk")]
    public void CuilInvalido_DebeReportarError(string cuil)
    {
        var request = new RegisterRequest(cuil, "Juan", "Pérez", "juan@test.com", "Password123!", "Password123!");
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Cuil");
    }

    [Fact]
    public void EmailInvalido_DebeReportarError()
    {
        var request = new RegisterRequest("20345678901", "Juan", "Pérez", "no-es-email", "Password123!", "Password123!");
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Email");
    }

    [Fact]
    public void PasswordsMismatch_DebeReportarError()
    {
        var request = new RegisterRequest("20345678901", "Juan", "Pérez", "juan@test.com", "Password123!", "OtraPassword!");
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "ConfirmPassword");
    }

    [Theory]
    [InlineData("short")]
    [InlineData("nouppercase1!")]
    [InlineData("NOLOWERCASE1!")]
    [InlineData("NoNumbers!")]
    public void PasswordDebil_DebeReportarError(string password)
    {
        var request = new RegisterRequest("20345678901", "Juan", "Pérez", "juan@test.com", password, password);
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void NombreVacio_DebeReportarError()
    {
        var request = new RegisterRequest("20345678901", "", "Pérez", "juan@test.com", "Password123!", "Password123!");
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Nombre");
    }

    [Fact]
    public void RequestCompleto_DebeSerValido()
    {
        var request = new RegisterRequest("20345678901", "Juan", "Pérez", "juan@test.com", "Password123!", "Password123!");
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
    }
}
