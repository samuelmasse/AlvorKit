namespace AlvorKit.Engine.Loop;

/// <summary>Forwards miniaudio calls while preventing agent runs from opening a real audio output device.</summary>
internal sealed unsafe class AgentSilentMa(Ma inner) : MaWrapper(inner)
{
    /// <inheritdoc />
    public override MaResult ContextInit(nint backends, uint backendCount, MaContextConfig* pConfig, MaContext* pContext)
    {
        var backend = MaBackendKind.BackendNull;
        return Inner.ContextInit((nint)(&backend), 1, pConfig, pContext);
    }

    /// <inheritdoc />
    public override MaResult DeviceInit(MaContext* pContext, MaDeviceConfig* pConfig, MaDevice* pDevice)
    {
        if (pContext != null)
            return Inner.DeviceInit(pContext, pConfig, pDevice);

        var backend = MaBackendKind.BackendNull;
        return Inner.DeviceInitEx((nint)(&backend), 1, null, pConfig, pDevice);
    }

    /// <inheritdoc />
    public override MaResult DeviceInitEx(
        nint backends,
        uint backendCount,
        MaContextConfig* pContextConfig,
        MaDeviceConfig* pConfig,
        MaDevice* pDevice)
    {
        var backend = MaBackendKind.BackendNull;
        return Inner.DeviceInitEx((nint)(&backend), 1, pContextConfig, pConfig, pDevice);
    }

    /// <inheritdoc />
    public override MaResult EngineInit(MaEngineConfig* pConfig, MaEngine* pEngine)
    {
        if (pConfig == null)
        {
            var config = Inner.EngineConfigInit();
            config.NoDevice = 1;
            return Inner.EngineInit(&config, pEngine);
        }

        var silentConfig = *pConfig;
        silentConfig.PDevice = null;
        silentConfig.NoDevice = 1;
        return Inner.EngineInit(&silentConfig, pEngine);
    }
}
