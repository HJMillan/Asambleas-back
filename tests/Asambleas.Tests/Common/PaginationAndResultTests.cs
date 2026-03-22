using Asambleas.Common;
using FluentAssertions;

namespace Asambleas.Tests.Common;

public class PaginationTests
{
    [Fact]
    public void PagedRequest_Skip_CalculaCorrectamente()
    {
        new PagedRequest(1, 20).Skip.Should().Be(0);
        new PagedRequest(2, 20).Skip.Should().Be(20);
        new PagedRequest(3, 10).Skip.Should().Be(20);
    }

    [Fact]
    public void PagedRequest_PageSize_ClampedEntre1y100()
    {
        new PagedRequest(1, 0).Take.Should().Be(1);
        new PagedRequest(1, -5).Take.Should().Be(1);
        new PagedRequest(1, 200).Take.Should().Be(100);
        new PagedRequest(1, 50).Take.Should().Be(50);
    }

    [Fact]
    public void PagedResponse_TotalPages_CalculaCorrectamente()
    {
        new PagedResponse<int>([], 0, 1, 20).TotalPages.Should().Be(0);
        new PagedResponse<int>([], 1, 1, 20).TotalPages.Should().Be(1);
        new PagedResponse<int>([], 21, 1, 20).TotalPages.Should().Be(2);
        new PagedResponse<int>([], 100, 1, 20).TotalPages.Should().Be(5);
    }

    [Fact]
    public void PagedResponse_HasPrevious_HasNext()
    {
        new PagedResponse<int>([], 100, 1, 20).HasPrevious.Should().BeFalse();
        new PagedResponse<int>([], 100, 2, 20).HasPrevious.Should().BeTrue();
        new PagedResponse<int>([], 100, 5, 20).HasNext.Should().BeFalse();
        new PagedResponse<int>([], 100, 3, 20).HasNext.Should().BeTrue();
    }
}

public class ResultTests
{
    [Fact]
    public void Success_DebeSerExitoso()
    {
        var result = Result<int>.Success(42);
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void Failure_DebeSerFallido()
    {
        var result = Result<int>.Failure("error msg");
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be("error msg");
    }

    [Fact]
    public void Match_EjecutaRamaCorrecta()
    {
        var success = Result<int>.Success(10);
        var matched = success.Match(v => $"ok:{v}", e => $"err:{e}");
        matched.Should().Be("ok:10");

        var failure = Result<int>.Failure("bad");
        var matched2 = failure.Match(v => $"ok:{v}", e => $"err:{e}");
        matched2.Should().Be("err:bad");
    }
}
