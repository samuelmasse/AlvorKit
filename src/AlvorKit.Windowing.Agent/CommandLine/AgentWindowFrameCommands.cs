namespace AlvorKit.Windowing;

/// <summary>Creates commands that advance deterministic agent time and rendering.</summary>
internal static class AgentWindowFrameCommands
{
    /// <summary>Adds frame-control commands to the root command.</summary>
    /// <param name="root">Root command that receives the subcommands.</param>
    /// <param name="protocol">Command protocol that receives parsed frame actions.</param>
    internal static void AddTo(RootCommand root, AgentWindowCommandProtocol protocol)
    {
        root.Subcommands.Add(CreateUpdateCommand(protocol));
        root.Subcommands.Add(CreateUpdatesCommand(protocol));
        root.Subcommands.Add(CreateRenderCommand(protocol));
        root.Subcommands.Add(CreateStepCommand(protocol));
    }

    /// <summary>Creates the single-update command.</summary>
    /// <param name="protocol">Command protocol that receives the parsed update.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateUpdateCommand(AgentWindowCommandProtocol protocol)
    {
        var delta = AgentWindowCommandArguments.NonNegativeDouble("delta");
        var mouseX = AgentWindowCommandArguments.OptionalFloat("mouseDx");
        var mouseY = AgentWindowCommandArguments.OptionalFloat("mouseDy");
        var command = new Command("update", "Run one update.");
        command.Arguments.Add(delta);
        command.Arguments.Add(mouseX);
        command.Arguments.Add(mouseY);
        command.SetAction(parseResult =>
        {
            if (protocol.TryOptionalVector(parseResult.GetValue(mouseX), parseResult.GetValue(mouseY), out var mouseDelta))
                protocol.Update(parseResult.GetValue(delta), mouseDelta);
        });
        return command;
    }

    /// <summary>Creates the fixed-update batch command.</summary>
    /// <param name="protocol">Command protocol that receives the parsed batch.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateUpdatesCommand(AgentWindowCommandProtocol protocol)
    {
        var count = AgentWindowCommandArguments.NonNegativeInt("count");
        var delta = AgentWindowCommandArguments.NonNegativeDouble("delta");
        var mouseX = AgentWindowCommandArguments.OptionalFloat("mouseDx");
        var mouseY = AgentWindowCommandArguments.OptionalFloat("mouseDy");
        var command = new Command("updates", "Run many updates.");
        command.Arguments.Add(count);
        command.Arguments.Add(delta);
        command.Arguments.Add(mouseX);
        command.Arguments.Add(mouseY);
        command.SetAction(parseResult =>
        {
            if (protocol.TryOptionalVector(parseResult.GetValue(mouseX), parseResult.GetValue(mouseY), out var mouseDelta))
                protocol.Advance(parseResult.GetValue(count), parseResult.GetValue(delta), mouseDelta);
        });
        return command;
    }

    /// <summary>Creates the render command.</summary>
    /// <param name="protocol">Command protocol that receives the parsed render.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateRenderCommand(AgentWindowCommandProtocol protocol)
    {
        var delta = AgentWindowCommandArguments.OptionalNonNegativeDouble("delta");
        var command = new Command("render", "Render one frame.");
        command.Arguments.Add(delta);
        command.SetAction(parseResult => protocol.Render(parseResult.GetValue(delta) ?? 0));
        return command;
    }

    /// <summary>Creates the update-plus-render command.</summary>
    /// <param name="protocol">Command protocol that receives the parsed step.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateStepCommand(AgentWindowCommandProtocol protocol)
    {
        var delta = AgentWindowCommandArguments.NonNegativeDouble("delta");
        var command = new Command("step", "Run one update and one render.");
        command.Arguments.Add(delta);
        command.SetAction(parseResult => protocol.Step(parseResult.GetValue(delta)));
        return command;
    }
}
