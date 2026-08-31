if (args is ["--verify"])
{
    var database = FastNoise2FeatureCatalog.Load();
    var verifier = new FastNoise2FeatureVerifier(new FnBackend(), database);
    verifier.Verify();
    return;
}

if (args.Length != 0)
    throw new ArgumentException("Usage: AlvorKit.FastNoise2.Demo [--verify]");

RootLoop.RunGlfw<FastNoise2DemoState>();
