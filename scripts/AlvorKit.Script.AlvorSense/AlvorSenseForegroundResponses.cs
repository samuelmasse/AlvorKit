namespace AlvorKit.Script.AlvorSense;

/// <summary>Writes foreground command JSON while adding requested local diagnostics.</summary>
internal static class AlvorSenseForegroundResponses
{
    /// <summary>Writes one foreground send response as JSON.</summary>
    /// <param name="response">Mailbox response written by the background host.</param>
    /// <param name="command">Parsed send command.</param>
    /// <param name="sessionDir">Session directory containing target logs.</param>
    /// <param name="output">Output stream receiving the JSON response.</param>
    internal static void WriteSend(
        AlvorSenseResponse response,
        AlvorSenseSendCommand command,
        string sessionDir,
        TextWriter output)
    {
        output.WriteLine(AlvorSenseJson.ToJson(AlvorSenseSendResult.From(response, StderrTail(command, sessionDir, response))));
        output.Flush();
    }

    /// <summary>Reads the requested stderr tail when the target exited during a failed send.</summary>
    private static string[]? StderrTail(AlvorSenseSendCommand command, string sessionDir, AlvorSenseResponse response)
    {
        if (command.StderrTailLines == 0 || response.Ok || !response.ProcessExited)
            return null;
        return AlvorSenseLogTail.Read(AlvorSensePaths.Stderr(sessionDir), command.StderrTailLines);
    }
}
