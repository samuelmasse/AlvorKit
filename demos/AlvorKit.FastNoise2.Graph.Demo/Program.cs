const int SampleCount = 4 * 3 * 2;

var fn = new FnBackend();
var oldSamples = new float[SampleCount];
var newSamples = new float[SampleCount];

new OldNoisePattern(fn).Sample(oldSamples);
new TypedNoisePattern(fn).Sample(newSamples);

if (!oldSamples.AsSpan().SequenceEqual(newSamples))
    throw new InvalidOperationException("The old and typed graph patterns produced different samples.");

Console.WriteLine("FastNoise2 raw metadata pattern and typed graph pattern produced identical output.");
Console.WriteLine(string.Join(", ", newSamples.Select(value => value.ToString("0.000000", CultureInfo.InvariantCulture))));
