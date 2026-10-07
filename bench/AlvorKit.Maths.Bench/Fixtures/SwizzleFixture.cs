namespace AlvorKit;

internal class SwizzleFixture
{
    private const int BatchSize = 4096;

    private readonly Vec3[] float3 = new Vec3[4096];
    private readonly Vec3[] float3Output = new Vec3[4096];
    private readonly Vec3i[] int3 = new Vec3i[4096];
    private readonly Vec3i[] int3Output = new Vec3i[4096];
    private readonly Vec4[] float4 = new Vec4[4096];
    private readonly Vec4[] float4Output = new Vec4[4096];
    private readonly Vec4i[] int4 = new Vec4i[4096];
    private readonly Vec4i[] int4Output = new Vec4i[4096];
    internal Vec3[] Float3 => float3;
    internal Vec3[] Float3Output => float3Output;
    internal Vec3i[] Int3 => int3;
    internal Vec3i[] Int3Output => int3Output;
    internal Vec4[] Float4 => float4;
    internal Vec4[] Float4Output => float4Output;
    internal Vec4i[] Int4 => int4;
    internal Vec4i[] Int4Output => int4Output;

    internal SwizzleFixture()
    {
        for (var i = 0; i < BatchSize; i++)
        {
            var x = (((i * 17) % 101) - 50) * 0.03125f;
            Float3[i] = (x + 1, x + 2, x + 3);
            Int3[i] = (i, i + 1, i + 2);
            Float4[i] = (x + 1, x + 2, x + 3, x + 4);
            Int4[i] = (i, i + 1, i + 2, i + 3);
        }
    }
}
