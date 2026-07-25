using MathVerse.Core;

namespace MathVerse.Kernel.Tests;

public class ErrorTests
{
    [Fact]
    public void Error_HasProperties()
    {
        var error = new Error("CODE", "message", ErrorKind.Validation);

        error.Code.Should().Be("CODE");
        error.Message.Should().Be("message");
        error.Kind.Should().Be(ErrorKind.Validation);
        error.Inner.Should().BeNull();
        error.Timestamp.Should().BeCloseTo(DateTimeOffset.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Error_WithInnerError()
    {
        var inner = new Error("INNER", "inner error", ErrorKind.Internal);
        var error = new Error("CODE", "message", ErrorKind.Internal, inner);

        error.Inner.Should().Be(inner);
    }

    [Fact]
    public void Error_FactoryMethods()
    {
        var validation = Error.Validation("V", "val err");
        validation.Kind.Should().Be(ErrorKind.Validation);

        var notFound = Error.NotFound("NF", "not found");
        notFound.Kind.Should().Be(ErrorKind.NotFound);

        var conflict = Error.Conflict("C", "conflict");
        conflict.Kind.Should().Be(ErrorKind.Conflict);

        var internal_ = Error.Internal("I", "internal");
        internal_.Kind.Should().Be(ErrorKind.Internal);
    }

    [Fact]
    public void Error_Equality()
    {
        var error1 = new Error("CODE", "msg", ErrorKind.Validation);
        var error2 = new Error("CODE", "msg", ErrorKind.Validation);

        error1.Should().Be(error2);
    }
}
