namespace AlvorKit;

/// <summary>Maps typed package vocabulary to exact FastNoise2 1.1.1 runtime metadata names.</summary>
internal static class FnNames
{
    /// <summary>Maps a node type to its case-sensitive runtime metadata name.</summary>
    public static string Node(FnNodeType value) => value switch
    {
        FnNodeType.Constant => "Constant",
        FnNodeType.White => "White",
        FnNodeType.Checkerboard => "Checkerboard",
        FnNodeType.SineWave => "SineWave",
        FnNodeType.Gradient => "Gradient",
        FnNodeType.DistanceToPoint => "DistanceToPoint",
        FnNodeType.Simplex => "Simplex",
        FnNodeType.SuperSimplex => "SuperSimplex",
        FnNodeType.Perlin => "Perlin",
        FnNodeType.Value => "Value",
        FnNodeType.CellularValue => "CellularValue",
        FnNodeType.CellularDistance => "CellularDistance",
        FnNodeType.CellularLookup => "CellularLookup",
        FnNodeType.FractalFbm => "FractalFBm",
        FnNodeType.PingPong => "PingPong",
        FnNodeType.FractalRidged => "FractalRidged",
        FnNodeType.DomainWarpSimplex => "DomainWarpSimplex",
        FnNodeType.DomainWarpSuperSimplex => "DomainWarpSuperSimplex",
        FnNodeType.DomainWarpGradient => "DomainWarpGradient",
        FnNodeType.DomainWarpFractalProgressive => "DomainWarpFractalProgressive",
        FnNodeType.DomainWarpFractalIndependent => "DomainWarpFractalIndependent",
        FnNodeType.Add => "Add",
        FnNodeType.Subtract => "Subtract",
        FnNodeType.Multiply => "Multiply",
        FnNodeType.Divide => "Divide",
        FnNodeType.Abs => "Abs",
        FnNodeType.Min => "Min",
        FnNodeType.Max => "Max",
        FnNodeType.MinSmooth => "MinSmooth",
        FnNodeType.MaxSmooth => "MaxSmooth",
        FnNodeType.SignedSquareRoot => "SignedSquareRoot",
        FnNodeType.PowFloat => "PowFloat",
        FnNodeType.PowInt => "PowInt",
        FnNodeType.DomainScale => "DomainScale",
        FnNodeType.DomainOffset => "DomainOffset",
        FnNodeType.DomainRotate => "DomainRotate",
        FnNodeType.DomainAxisScale => "DomainAxisScale",
        FnNodeType.SeedOffset => "SeedOffset",
        FnNodeType.ConvertRgba8 => "ConvertRGBA8",
        FnNodeType.GeneratorCache => "GeneratorCache",
        FnNodeType.Fade => "Fade",
        FnNodeType.Remap => "Remap",
        FnNodeType.Terrace => "Terrace",
        FnNodeType.AddDimension => "AddDimension",
        FnNodeType.RemoveDimension => "RemoveDimension",
        FnNodeType.Modulus => "Modulus",
        FnNodeType.DomainRotatePlane => "DomainRotatePlane",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    /// <summary>Maps a float variable to its case-sensitive name and component.</summary>
    public static FnMemberKey Float(FnFloatVariable value) => value switch
    {
        FnFloatVariable.AmplitudeScalingX => new("Amplitude Scaling", 0),
        FnFloatVariable.AmplitudeScalingY => new("Amplitude Scaling", 1),
        FnFloatVariable.AmplitudeScalingZ => new("Amplitude Scaling", 2),
        FnFloatVariable.AmplitudeScalingW => new("Amplitude Scaling", 3),
        FnFloatVariable.FeatureScale => FnMemberKey.Scalar("Feature Scale"),
        FnFloatVariable.Lacunarity => FnMemberKey.Scalar("Lacunarity"),
        FnFloatVariable.Maximum => FnMemberKey.Scalar("Max"),
        FnFloatVariable.Minimum => FnMemberKey.Scalar("Min"),
        FnFloatVariable.MultiplierX => new("Multiplier", 0),
        FnFloatVariable.MultiplierY => new("Multiplier", 1),
        FnFloatVariable.MultiplierZ => new("Multiplier", 2),
        FnFloatVariable.MultiplierW => new("Multiplier", 3),
        FnFloatVariable.OutputMaximum => FnMemberKey.Scalar("Output Max"),
        FnFloatVariable.OutputMinimum => FnMemberKey.Scalar("Output Min"),
        FnFloatVariable.Pitch => FnMemberKey.Scalar("Pitch"),
        FnFloatVariable.Roll => FnMemberKey.Scalar("Roll"),
        FnFloatVariable.Scaling => FnMemberKey.Scalar("Scaling"),
        FnFloatVariable.ScalingX => new("Scaling", 0),
        FnFloatVariable.ScalingY => new("Scaling", 1),
        FnFloatVariable.ScalingZ => new("Scaling", 2),
        FnFloatVariable.ScalingW => new("Scaling", 3),
        FnFloatVariable.StepCount => FnMemberKey.Scalar("Step Count"),
        FnFloatVariable.Value => FnMemberKey.Scalar("Value"),
        FnFloatVariable.Yaw => FnMemberKey.Scalar("Yaw"),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    /// <summary>Maps an integer variable to its case-sensitive scalar name.</summary>
    public static FnMemberKey Integer(FnIntegerVariable value) => value switch
    {
        FnIntegerVariable.SeedOffset => FnMemberKey.Scalar("Seed Offset"),
        FnIntegerVariable.ValueIndex => FnMemberKey.Scalar("Value Index"),
        FnIntegerVariable.DistanceIndex0 => FnMemberKey.Scalar("Distance Index 0"),
        FnIntegerVariable.DistanceIndex1 => FnMemberKey.Scalar("Distance Index 1"),
        FnIntegerVariable.Octaves => FnMemberKey.Scalar("Octaves"),
        FnIntegerVariable.Power => FnMemberKey.Scalar("Pow"),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    /// <summary>Maps a hybrid input to its case-sensitive name and component.</summary>
    public static FnMemberKey Hybrid(FnHybrid value) => value switch
    {
        FnHybrid.Fade => FnMemberKey.Scalar("Fade"),
        FnHybrid.FadeMaximum => FnMemberKey.Scalar("Fade Max"),
        FnHybrid.FadeMinimum => FnMemberKey.Scalar("Fade Min"),
        FnHybrid.FromMaximum => FnMemberKey.Scalar("From Max"),
        FnHybrid.FromMinimum => FnMemberKey.Scalar("From Min"),
        FnHybrid.Gain => FnMemberKey.Scalar("Gain"),
        FnHybrid.GridJitter => FnMemberKey.Scalar("Grid Jitter"),
        FnHybrid.Lhs => FnMemberKey.Scalar("LHS"),
        FnHybrid.MinkowskiP => FnMemberKey.Scalar("Minkowski P"),
        FnHybrid.NewDimensionPosition => FnMemberKey.Scalar("New Dimension Position"),
        FnHybrid.OffsetX => new("Offset", 0),
        FnHybrid.OffsetY => new("Offset", 1),
        FnHybrid.OffsetZ => new("Offset", 2),
        FnHybrid.OffsetW => new("Offset", 3),
        FnHybrid.PingPongStrength => FnMemberKey.Scalar("Ping Pong Strength"),
        FnHybrid.PointX => new("Point", 0),
        FnHybrid.PointY => new("Point", 1),
        FnHybrid.PointZ => new("Point", 2),
        FnHybrid.PointW => new("Point", 3),
        FnHybrid.Power => FnMemberKey.Scalar("Pow"),
        FnHybrid.Rhs => FnMemberKey.Scalar("RHS"),
        FnHybrid.SizeJitter => FnMemberKey.Scalar("Size Jitter"),
        FnHybrid.Smoothness => FnMemberKey.Scalar("Smoothness"),
        FnHybrid.ToMaximum => FnMemberKey.Scalar("To Max"),
        FnHybrid.ToMinimum => FnMemberKey.Scalar("To Min"),
        FnHybrid.Value => FnMemberKey.Scalar("Value"),
        FnHybrid.WarpAmplitude => FnMemberKey.Scalar("Warp Amplitude"),
        FnHybrid.WeightedStrength => FnMemberKey.Scalar("Weighted Strength"),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    /// <summary>Maps a required input to its case-sensitive scalar name.</summary>
    public static FnMemberKey Source(FnSource value) => value switch
    {
        FnSource.A => FnMemberKey.Scalar("A"),
        FnSource.B => FnMemberKey.Scalar("B"),
        FnSource.DomainWarpSource => FnMemberKey.Scalar("Domain Warp Source"),
        FnSource.Lhs => FnMemberKey.Scalar("LHS"),
        FnSource.Lookup => FnMemberKey.Scalar("Lookup"),
        FnSource.Source => FnMemberKey.Scalar("Source"),
        FnSource.Value => FnMemberKey.Scalar("Value"),
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    /// <summary>Maps a distance function to its case-sensitive runtime option label.</summary>
    public static string DistanceFunction(FnDistanceFunction value) => value switch
    {
        FnDistanceFunction.Euclidean => "Euclidean",
        FnDistanceFunction.EuclideanSquared => "Euclidean Squared",
        FnDistanceFunction.Manhattan => "Manhattan",
        FnDistanceFunction.Hybrid => "Hybrid",
        FnDistanceFunction.MaximumAxis => "Max Axis",
        FnDistanceFunction.Minkowski => "Minkowski",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    /// <summary>Maps a cellular return operation to its case-sensitive runtime option label.</summary>
    public static string CellularReturnType(FnCellularReturnType value) => value switch
    {
        FnCellularReturnType.Index0 => "Index0",
        FnCellularReturnType.Index0Add1 => "Index0Add1",
        FnCellularReturnType.Index0AbsoluteDifference1 => "Index0Sub1",
        FnCellularReturnType.Index0Multiply1 => "Index0Mul1",
        FnCellularReturnType.Index0Divide1 => "Index0Div1",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    /// <summary>Maps a fade interpolation curve to its case-sensitive runtime option label.</summary>
    public static string Interpolation(FnInterpolation value) => value switch
    {
        FnInterpolation.Linear => "Linear",
        FnInterpolation.Hermite => "Hermite",
        FnInterpolation.Quintic => "Quintic",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    /// <summary>Maps a removed coordinate to its case-sensitive runtime option label.</summary>
    public static string RemovedDimension(FnRemovedDimension value) => value switch
    {
        FnRemovedDimension.X => "X",
        FnRemovedDimension.Y => "Y",
        FnRemovedDimension.Z => "Z",
        FnRemovedDimension.W => "W",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    /// <summary>Maps a plane rotation preset to its case-sensitive runtime option label.</summary>
    public static string RotationType(FnRotationType value) => value switch
    {
        FnRotationType.ImproveXyPlanes => "Improve XY Planes",
        FnRotationType.ImproveXzPlanes => "Improve XZ Planes",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };

    /// <summary>Maps a domain-warp vector scheme to its case-sensitive runtime option label.</summary>
    public static string VectorizationScheme(FnVectorizationScheme value) => value switch
    {
        FnVectorizationScheme.OrthogonalGradientMatrix => "Orthogonal Gradient Matrix",
        FnVectorizationScheme.GradientOuterProduct => "Gradient Outer Product",
        _ => throw new ArgumentOutOfRangeException(nameof(value), value, null),
    };
}
