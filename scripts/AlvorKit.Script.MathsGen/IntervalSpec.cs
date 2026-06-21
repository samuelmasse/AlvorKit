namespace AlvorKit.Script.MathsGen;

/// <summary>Describes one generated scalar interval type.</summary>
internal sealed record IntervalSpec(ScalarSpec Scalar)
{
    /// <summary>Gets the generated interval type name.</summary>
    public string TypeName => Scalar.IntervalName();

    /// <summary>Gets the byte size of the interval.</summary>
    public int SizeBytes => Scalar.SizeBytes * 2;
}
