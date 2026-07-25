namespace MathVerse.Math.Foundation.Constants;

public sealed class ConstantProvider
{
    private readonly Func<double> _numericProvider;

    private readonly Func<Complex> _complexProvider;

    public ConstantProvider(Func<double> numericProvider, Func<Complex> complexProvider)
    {
        _numericProvider = numericProvider ?? throw new ArgumentNullException(nameof(numericProvider));
        _complexProvider = complexProvider ?? throw new ArgumentNullException(nameof(complexProvider));
    }

    public double GetNumeric()
    {
        return _numericProvider();
    }

    public Complex GetComplex()
    {
        return _complexProvider();
    }
}
