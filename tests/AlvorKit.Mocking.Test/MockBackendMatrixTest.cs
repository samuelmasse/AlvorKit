namespace AlvorKit.Mocking.Test;

[TestClass]
public sealed class MockBackendMatrixTest
{
    /// <summary>
    /// Concurrent proxy receivers retain instance-local behavior and history.
    /// </summary>
    [TestMethod]
    public void ConcurrentBackends_MockedAndUnmockedReceiversRemainIsolated()
    {
        const int iterationCount = 32;
        var firstProxy = Mock.Create<IMockTarget>();
        var secondProxy = Mock.Create<IMockTarget>();
        var results = new int[iterationCount, 2];

        Mock.When(firstProxy.GetValue).Return(11);
        Mock.When(secondProxy.GetValue).Return(22);
        Parallel.For(
            0,
            iterationCount,
            index =>
            {
                results[index, 0] = firstProxy.GetValue();
                results[index, 1] = secondProxy.GetValue();
            });

        for (var index = 0; index < iterationCount; index++)
        {
            Assert.AreEqual(11, results[index, 0]);
            Assert.AreEqual(22, results[index, 1]);
        }

        Mock.Verify(firstProxy.GetValue).Exactly(iterationCount);
        Mock.Verify(secondProxy.GetValue).Exactly(iterationCount);
        Mock.VerifyNoOtherCalls(firstProxy);
        Mock.VerifyNoOtherCalls(secondProxy);
    }
}
