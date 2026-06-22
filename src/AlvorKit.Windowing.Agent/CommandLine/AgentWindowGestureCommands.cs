namespace AlvorKit.Windowing;

/// <summary>Creates compact deterministic mouse gesture commands.</summary>
internal static class AgentWindowGestureCommands
{
    /// <summary>Adds gesture commands to the root command.</summary>
    /// <param name="root">Root command that receives the subcommands.</param>
    /// <param name="protocol">Command protocol that receives parsed gesture actions.</param>
    internal static void AddTo(RootCommand root, AgentWindowCommandProtocol protocol)
    {
        root.Subcommands.Add(CreateClickCommand(protocol));
        root.Subcommands.Add(CreateDragCommand(protocol));
    }

    private static Command CreateClickCommand(AgentWindowCommandProtocol protocol)
    {
        var x = AgentWindowCommandArguments.Float("x");
        var y = AgentWindowCommandArguments.Float("y");
        var delta = AgentWindowCommandArguments.OptionalNonNegativeDouble("delta");
        var command = new Command("click", "Run a one-frame left-button click.");
        command.Arguments.Add(x);
        command.Arguments.Add(y);
        command.Arguments.Add(delta);
        command.SetAction(parse => protocol.Click(
            new(parse.GetValue(x), parse.GetValue(y)),
            parse.GetValue(delta) ?? AgentWindowGestureDriver.DefaultDelta));
        return command;
    }

    private static Command CreateDragCommand(AgentWindowCommandProtocol protocol)
    {
        var startX = AgentWindowCommandArguments.Float("startX");
        var startY = AgentWindowCommandArguments.Float("startY");
        var endX = AgentWindowCommandArguments.Float("endX");
        var endY = AgentWindowCommandArguments.Float("endY");
        var steps = AgentWindowCommandArguments.PositiveInt("steps");
        var delta = AgentWindowCommandArguments.NonNegativeDouble("delta");
        var command = new Command("drag", "Run a left-button drag over fixed updates.");
        command.Arguments.Add(startX);
        command.Arguments.Add(startY);
        command.Arguments.Add(endX);
        command.Arguments.Add(endY);
        command.Arguments.Add(steps);
        command.Arguments.Add(delta);
        command.SetAction(parse => protocol.Drag(
            new(parse.GetValue(startX), parse.GetValue(startY)),
            new(parse.GetValue(endX), parse.GetValue(endY)),
            parse.GetValue(steps),
            parse.GetValue(delta)));
        return command;
    }
}
