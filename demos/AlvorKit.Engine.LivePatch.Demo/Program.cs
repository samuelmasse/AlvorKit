if (args is ["--proof"])
{
    using var logging = new LogRuntime();
    logging.Start();
    return LivePatchProof.Run(logging.Log);
}

RootLoop.RunGlfw<LivePatchDemoState>();
return 0;
