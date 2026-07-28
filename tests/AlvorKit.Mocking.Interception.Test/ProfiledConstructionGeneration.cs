namespace AlvorKit.Mocking.Interception.Test;

/// <summary>Builds and installs one production generation for a selected newobj site.</summary>
internal static class ProfiledConstructionGeneration
{
    /// <summary>Loads, recognizes, lowers, and installs one authoritative caller body.</summary>
    internal static IInterceptionPatchHandle Install(
        IInterceptionBackend backend,
        MethodInfo caller,
        ConstructorInfo constructor,
        MethodInfo route)
    {
        if (backend is not InterceptionProfiler profiler)
        {
            throw new InvalidOperationException(
                "Production construction lowering requires the CoreCLR " +
                "loaded-body backend.");
        }

        RuntimeHelpers.PrepareMethod(caller.MethodHandle);
        InterceptionTarget target = InterceptionTarget.FromMethod(caller);
        LoadedMethodBodySnapshot body =
            profiler.GetLoadedMethodBody(target);
        var resolver =
            new ReflectionLoadedOperationMetadataResolver(caller);
        LoadedOperationRecognition recognition =
            LoadedOperationRecognizer.Recognize(
                body,
                caller.Module.ModuleVersionId,
                caller.MetadataToken,
                resolver,
                resolver.ConstructedContext);
        if (!recognition.IsSuccessful)
        {
            throw new InvalidOperationException(
                string.Join(
                    Environment.NewLine,
                    recognition.Rejections.Select(rejection =>
                        rejection.Detail)));
        }

        int operationOffset =
            ProfiledReceiverFreeOperationOffset.Find(
                caller,
                constructor);
        LoadedOperationSiteDescriptor site = recognition.Sites
            .Single(candidate =>
                candidate.Kind ==
                    LoadedOperationKind.ObjectConstruction &&
                candidate.BaselineOffset == operationOffset);
        InterceptionGenerationPlan generation =
            LoadedConstructionCallerComposer.Compose(
                caller,
                body,
                site,
                constructor,
                route,
                1);
        return profiler.Install(generation);
    }
}
