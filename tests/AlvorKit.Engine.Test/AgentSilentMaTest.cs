namespace AlvorKit.Engine.Test;

[TestClass]
public sealed unsafe class AgentSilentMaTest
{
    /// <summary>Null engine configs are replaced with a no-device config before the real backend is called.</summary>
    [TestMethod]
    public void EngineInit_NullConfig_ForwardsNoDeviceConfig()
    {
        var inner = new CapturingMa();
        var ma = new AgentSilentMa(inner);
        var engine = default(MaEngine);

        var result = ma.EngineInit(null, &engine);

        Assert.AreEqual(MaResult.Success, result);
        Assert.IsFalse(inner.ReceivedNullEngineConfig);
        Assert.IsNotNull(inner.EngineConfig);
        Assert.AreEqual(1u, inner.EngineConfig.Value.NoDevice);
    }

    /// <summary>Supplied engine configs are copied and silenced without mutating the caller-owned config.</summary>
    [TestMethod]
    public void EngineInit_CustomConfig_ForwardsCopiedNoDeviceConfig()
    {
        var inner = new CapturingMa();
        var ma = new AgentSilentMa(inner);
        var engine = default(MaEngine);
        var config = new MaEngineConfig { Channels = 2, SampleRate = 48000, NoDevice = 0 };

        var result = ma.EngineInit(&config, &engine);

        Assert.AreEqual(MaResult.Success, result);
        Assert.AreEqual(0u, config.NoDevice);
        Assert.IsNotNull(inner.EngineConfig);
        Assert.AreEqual(2u, inner.EngineConfig.Value.Channels);
        Assert.AreEqual(48000u, inner.EngineConfig.Value.SampleRate);
        Assert.AreEqual(1u, inner.EngineConfig.Value.NoDevice);
    }

    /// <summary>Default RootLoop audio is silent for agent runs unless the silent-audio flag is disabled.</summary>
    [TestMethod]
    public void CreateAudioBackend_UsesSilentWrapperForAgentOrSilentEnvironment()
    {
        var oldAudioValue = Environment.GetEnvironmentVariable(RootLoop.AudioSilentEnvironmentVariable);
        var oldAgentValue = Environment.GetEnvironmentVariable(AgentGlfwWindowHost.AgentEnvironmentVariable);
        try
        {
            Environment.SetEnvironmentVariable(RootLoop.AudioSilentEnvironmentVariable, null);
            Environment.SetEnvironmentVariable(AgentGlfwWindowHost.AgentEnvironmentVariable, null);
            Assert.IsInstanceOfType<MaBackend>(RootLoop.CreateAudioBackend());

            Environment.SetEnvironmentVariable(AgentGlfwWindowHost.AgentEnvironmentVariable, "1");
            Assert.IsInstanceOfType<AgentSilentMa>(RootLoop.CreateAudioBackend());

            Environment.SetEnvironmentVariable(RootLoop.AudioSilentEnvironmentVariable, "0");
            Assert.IsInstanceOfType<MaBackend>(RootLoop.CreateAudioBackend());

            Environment.SetEnvironmentVariable(RootLoop.AudioSilentEnvironmentVariable, "1");
            Assert.IsInstanceOfType<AgentSilentMa>(RootLoop.CreateAudioBackend());
        }
        finally
        {
            Environment.SetEnvironmentVariable(RootLoop.AudioSilentEnvironmentVariable, oldAudioValue);
            Environment.SetEnvironmentVariable(AgentGlfwWindowHost.AgentEnvironmentVariable, oldAgentValue);
        }
    }

    private sealed class CapturingMa : Ma
    {
        public MaEngineConfig? EngineConfig { get; private set; }

        public bool ReceivedNullEngineConfig { get; private set; }

        public override MaEngineConfig EngineConfigInit() => default;

        public override MaResult EngineInit(MaEngineConfig* pConfig, MaEngine* pEngine)
        {
            ReceivedNullEngineConfig = pConfig == null;
            EngineConfig = pConfig == null ? null : *pConfig;
            return MaResult.Success;
        }
    }
}
