if (args is ["--proof"])
    return LivePatchProof.Run();

RootLoop.RunGlfw<LivePatchDemoState>();
return 0;
