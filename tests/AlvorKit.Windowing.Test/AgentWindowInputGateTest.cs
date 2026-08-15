namespace AlvorKit;

/// <summary>Tests exclusive synthetic input and post-release native poll quarantine.</summary>
[TestClass]
public sealed class AgentWindowInputGateTest
{
    /// <summary>A gate permits only one reservation and releases it idempotently.</summary>
    [TestMethod]
    public void Reserve_WithActiveOwner_RejectsOverlapAndReleasesOnce()
    {
        AgentWindowInputGate gate = new();
        var reservation = gate.Reserve();

        Assert.IsTrue(gate.IsReserved);
        Assert.IsFalse(gate.AcceptsNativeEvents);
        Assert.ThrowsException<InvalidOperationException>(() => gate.Reserve());

        reservation.Dispose();
        reservation.Dispose();

        Assert.IsFalse(gate.IsReserved);
        Assert.IsTrue(gate.AcceptsNativeEvents);
    }

    /// <summary>The first native poll after release is quarantined and later polls are accepted.</summary>
    [TestMethod]
    public void Poll_AfterRelease_QuarantinesExactlyOnePoll()
    {
        AgentWindowInputGate gate = new();
        gate.Reserve().Dispose();

        gate.BeforePoll();
        Assert.IsFalse(gate.AcceptsNativeEvents);
        gate.AfterPoll();

        Assert.IsTrue(gate.AcceptsNativeEvents);
        gate.BeforePoll();
        Assert.IsTrue(gate.AcceptsNativeEvents);
    }
}
