namespace MathVerse.Math.Types.Visitors;

/// <summary>Visitor interface for mathematical types.</summary>
public interface ITypeVisitor<T>
{
    /// <summary>Visit an unknown type.</summary>
    T VisitUnknown(UnknownType type);
    /// <summary>Visit an error type.</summary>
    T VisitError(ErrorType type);
    /// <summary>Visit a unit type.</summary>
    T VisitUnit(UnitType type);
    /// <summary>Visit a boolean type.</summary>
    T VisitBoolean(BooleanType type);
    /// <summary>Visit an integer type.</summary>
    T VisitInteger(IntegerType type);
    /// <summary>Visit a typed integer.</summary>
    T VisitTypedInteger(TypedInteger type);
    /// <summary>Visit a rational type.</summary>
    T VisitRational(RationalType type);
    /// <summary>Visit a real type.</summary>
    T VisitReal(RealType type);
    /// <summary>Visit a complex type.</summary>
    T VisitComplex(ComplexType type);
    /// <summary>Visit a string type.</summary>
    T VisitString(StringType type);
    /// <summary>Visit a function type.</summary>
    T VisitFunction(FunctionType type);
    /// <summary>Visit a vector type.</summary>
    T VisitVector(VectorType type);
    /// <summary>Visit a matrix type.</summary>
    T VisitMatrix(MatrixType type);
    /// <summary>Visit a tensor type.</summary>
    T VisitTensor(TensorType type);
    /// <summary>Visit a polynomial type.</summary>
    T VisitPolynomial(PolynomialType type);
    /// <summary>Visit an equation type.</summary>
    T VisitEquation(EquationType type);
    /// <summary>Visit a set type.</summary>
    T VisitSet(SetType type);
    /// <summary>Visit a tuple type.</summary>
    T VisitTuple(TupleType type);
    /// <summary>Visit a sequence type.</summary>
    T VisitSequence(SequenceType type);
    /// <summary>Visit a domain type.</summary>
    T VisitDomain(DomainType type);
    /// <summary>Visit a type parameter.</summary>
    T VisitTypeParameter(TypeParameter type);
    /// <summary>Visit a generic type definition.</summary>
    T VisitGenericType(GenericType type);
    /// <summary>Visit a generic instantiation.</summary>
    T VisitGenericInstantiation(GenericInstantiation type);
    /// <summary>Visit a type variable.</summary>
    T VisitTypeVariable(TypeVariable type);
}

/// <summary>Default implementation of ITypeVisitor that dispatches by Kind.</summary>
public abstract class TypeVisitor<T> : ITypeVisitor<T>
{
    /// <summary>Dispatches to the appropriate visit method.</summary>
    public T Visit(MathType type)
    {
        return type switch
        {
            UnknownType t => VisitUnknown(t),
            ErrorType t => VisitError(t),
            UnitType t => VisitUnit(t),
            BooleanType t => VisitBoolean(t),
            IntegerType t => VisitInteger(t),
            TypedInteger t => VisitTypedInteger(t),
            RationalType t => VisitRational(t),
            RealType t => VisitReal(t),
            ComplexType t => VisitComplex(t),
            StringType t => VisitString(t),
            FunctionType t => VisitFunction(t),
            VectorType t => VisitVector(t),
            MatrixType t => VisitMatrix(t),
            TensorType t => VisitTensor(t),
            PolynomialType t => VisitPolynomial(t),
            EquationType t => VisitEquation(t),
            SetType t => VisitSet(t),
            TupleType t => VisitTuple(t),
            SequenceType t => VisitSequence(t),
            DomainType t => VisitDomain(t),
            TypeParameter t => VisitTypeParameter(t),
            GenericType t => VisitGenericType(t),
            GenericInstantiation t => VisitGenericInstantiation(t),
            TypeVariable t => VisitTypeVariable(t),
            _ => VisitDefault(type),
        };
    }

    /// <summary>Default handler for unrecognized types.</summary>
    protected virtual T VisitDefault(MathType type) =>
        throw new System.NotSupportedException($"Unsupported type: {type.GetType().Name}");

    /// <inheritdoc/>
    public abstract T VisitUnknown(UnknownType type);
    /// <inheritdoc/>
    public abstract T VisitError(ErrorType type);
    /// <inheritdoc/>
    public abstract T VisitUnit(UnitType type);
    /// <inheritdoc/>
    public abstract T VisitBoolean(BooleanType type);
    /// <inheritdoc/>
    public abstract T VisitInteger(IntegerType type);
    /// <inheritdoc/>
    public abstract T VisitTypedInteger(TypedInteger type);
    /// <inheritdoc/>
    public abstract T VisitRational(RationalType type);
    /// <inheritdoc/>
    public abstract T VisitReal(RealType type);
    /// <inheritdoc/>
    public abstract T VisitComplex(ComplexType type);
    /// <inheritdoc/>
    public abstract T VisitString(StringType type);
    /// <inheritdoc/>
    public abstract T VisitFunction(FunctionType type);
    /// <inheritdoc/>
    public abstract T VisitVector(VectorType type);
    /// <inheritdoc/>
    public abstract T VisitMatrix(MatrixType type);
    /// <inheritdoc/>
    public abstract T VisitTensor(TensorType type);
    /// <inheritdoc/>
    public abstract T VisitPolynomial(PolynomialType type);
    /// <inheritdoc/>
    public abstract T VisitEquation(EquationType type);
    /// <inheritdoc/>
    public abstract T VisitSet(SetType type);
    /// <inheritdoc/>
    public abstract T VisitTuple(TupleType type);
    /// <inheritdoc/>
    public abstract T VisitSequence(SequenceType type);
    /// <inheritdoc/>
    public abstract T VisitDomain(DomainType type);
    /// <inheritdoc/>
    public abstract T VisitTypeParameter(TypeParameter type);
    /// <inheritdoc/>
    public abstract T VisitGenericType(GenericType type);
    /// <inheritdoc/>
    public abstract T VisitGenericInstantiation(GenericInstantiation type);
    /// <inheritdoc/>
    public abstract T VisitTypeVariable(TypeVariable type);
}
