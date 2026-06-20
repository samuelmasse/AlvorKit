namespace AlvorKit.Script.AlvorSense;

/// <summary>Implements the foreground side of the filesystem mailbox used to talk to a session host.</summary>
internal static class AlvorSenseRequestStore
{
    /// <summary>Sends a request to a running host and waits for the matching response.</summary>
    internal static AlvorSenseResponse Send(string sessionDir, AlvorSenseRequest request, TimeSpan timeout)
    {
        if (!File.Exists(AlvorSensePaths.Ready(sessionDir)))
            throw new InvalidOperationException($"Session is not ready: {sessionDir}");

        AlvorSenseJson.Save(AlvorSensePaths.RequestTemp(sessionDir, request.Id), request);
        File.Move(AlvorSensePaths.RequestTemp(sessionDir, request.Id), AlvorSensePaths.Request(sessionDir, request.Id));
        return WaitResponse(sessionDir, request.Id, timeout);
    }

    /// <summary>Waits until a response file appears or the timeout elapses.</summary>
    internal static AlvorSenseResponse WaitResponse(string sessionDir, string id, TimeSpan timeout)
    {
        var responsePath = AlvorSensePaths.Response(sessionDir, id);
        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            if (File.Exists(responsePath))
                return AlvorSenseJson.Load<AlvorSenseResponse>(responsePath);
            Thread.Sleep(50);
        }

        throw new TimeoutException($"Timed out waiting for response {id}.");
    }
}
