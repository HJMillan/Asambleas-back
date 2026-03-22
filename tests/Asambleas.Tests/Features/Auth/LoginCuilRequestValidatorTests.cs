using Asambleas.Features.Auth;
using Asambleas.Features.Auth.Validators;
using FluentAssertions;

namespace Asambleas.Tests.Features.Auth;

/// <summary>
/// Tests de validación para LoginCuilRequest.
/// </summary>
public class LoginCuilRequestValidatorTests
{
    private readonly LoginCuilRequestValidator _sut = new();

    [Fact]
    public void CuilYPasswordValidos_DebeSerValido()
    {
        var request = new LoginCuilRequest("20345678901", "Password123!");
        var result = _sut.Validate(request);
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("", "Password123!")]
    [InlineData("123", "Password123!")]
    [InlineData("20345678901", "")]
    public void DatosInvalidos_DebeReportarError(string cuil, string password)
    {
        var request = new LoginCuilRequest(cuil, password);
        var result = _sut.Validate(request);
        result.IsValid.Should().BeFalse();
    }
}
