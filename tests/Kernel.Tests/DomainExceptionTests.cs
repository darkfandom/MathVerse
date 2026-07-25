using MathVerse.Core;

namespace MathVerse.Kernel.Tests;

public class DomainExceptionTests
{
    [Fact]
    public void DomainException_HasCodeAndMessage()
    {
        var ex = new DomainException("ERR_001", "Something went wrong");

        ex.Code.Should().Be("ERR_001");
        ex.Message.Should().Be("Something went wrong");
    }

    [Fact]
    public void DomainException_WithInnerException()
    {
        var inner = new InvalidOperationException("inner");
        var ex = new DomainException("ERR_001", "Something went wrong", inner);

        ex.InnerException.Should().Be(inner);
    }

    [Fact]
    public void DomainException_ToError()
    {
        var ex = new DomainException("ERR_001", "Something went wrong");
        var error = ex.ToError();

        error.Code.Should().Be("ERR_001");
        error.Message.Should().Be("Something went wrong");
        error.Kind.Should().Be(ErrorKind.Internal);
    }
}
