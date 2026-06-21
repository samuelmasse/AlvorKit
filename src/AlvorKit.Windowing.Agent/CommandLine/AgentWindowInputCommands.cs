namespace AlvorKit.Windowing;

/// <summary>Creates commands that inject deterministic input into the agent window.</summary>
internal static class AgentWindowInputCommands
{
    /// <summary>Adds input and window-state commands to the root command.</summary>
    /// <param name="root">Root command that receives the subcommands.</param>
    /// <param name="protocol">Command protocol that receives parsed input actions.</param>
    internal static void AddTo(RootCommand root, AgentWindowCommandProtocol protocol)
    {
        root.Subcommands.Add(CreateKeyCommand(protocol));
        root.Subcommands.Add(CreateMouseCommand(protocol));
        root.Subcommands.Add(CreateVectorCommand("move", "Move the cursor to absolute client coordinates.", protocol.MoveMouse));
        root.Subcommands.Add(CreateVectorCommand("pan", "Move the cursor by a client-space delta.", protocol.PanMouse));
        root.Subcommands.Add(CreateVectorCommand("wheel", "Inject mouse wheel delta.", protocol.ScrollMouse));
        root.Subcommands.Add(CreateSizeCommand(protocol));
        root.Subcommands.Add(CreateTextCommand(protocol));
        root.Subcommands.Add(CreateClipboardCommand(protocol));
        root.Subcommands.Add(CreateBoolCommand("focus", "Set whether the window reports focus.", protocol.SetFocus));
        root.Subcommands.Add(CreateBoolCommand("visible", "Set whether the window reports visibility.", protocol.SetVisible));
    }

    /// <summary>Creates the keyboard input command.</summary>
    /// <param name="protocol">Command protocol that receives the parsed key action.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateKeyCommand(AgentWindowCommandProtocol protocol)
    {
        var key = AgentWindowCommandArguments.Enum<Keys>("name");
        var action = AgentWindowCommandArguments.Enum<AgentWindowKeyCommandAction>("action");
        var command = new Command("key", "Inject key input.");
        command.Arguments.Add(key);
        command.Arguments.Add(action);
        command.SetAction(parseResult => protocol.Key(parseResult.GetValue(key), parseResult.GetValue(action)));
        return command;
    }

    /// <summary>Creates the mouse button input command.</summary>
    /// <param name="protocol">Command protocol that receives the parsed mouse action.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateMouseCommand(AgentWindowCommandProtocol protocol)
    {
        var button = AgentWindowCommandArguments.Enum<MouseButton>("button");
        var action = AgentWindowCommandArguments.Enum<AgentWindowMouseCommandAction>("action");
        var command = new Command("mouse", "Inject mouse button input.");
        command.Arguments.Add(button);
        command.Arguments.Add(action);
        command.SetAction(parseResult => protocol.Mouse(parseResult.GetValue(button), parseResult.GetValue(action)));
        return command;
    }

    /// <summary>Creates a two-component vector command.</summary>
    /// <param name="name">Command name.</param>
    /// <param name="description">Help description.</param>
    /// <param name="action">Protocol action that receives the parsed vector.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateVectorCommand(string name, string description, Action<Vec2> action)
    {
        var x = AgentWindowCommandArguments.Float("x");
        var y = AgentWindowCommandArguments.Float("y");
        var command = new Command(name, description);
        command.Arguments.Add(x);
        command.Arguments.Add(y);
        command.SetAction(parseResult => action(new(parseResult.GetValue(x), parseResult.GetValue(y))));
        return command;
    }

    /// <summary>Creates the integer client resize command.</summary>
    /// <param name="protocol">Command protocol that receives the parsed size.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateSizeCommand(AgentWindowCommandProtocol protocol)
    {
        var width = AgentWindowCommandArguments.PositiveInt("width");
        var height = AgentWindowCommandArguments.PositiveInt("height");
        var command = new Command("resize", "Resize the client area.");
        command.Arguments.Add(width);
        command.Arguments.Add(height);
        command.SetAction(parseResult => protocol.ResizeWindow(new(
            checked((uint)parseResult.GetValue(width)),
            checked((uint)parseResult.GetValue(height)))));
        return command;
    }

    /// <summary>Creates the text input command.</summary>
    /// <param name="protocol">Command protocol that receives the parsed text.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateTextCommand(AgentWindowCommandProtocol protocol)
    {
        var value = AgentWindowCommandArguments.Words("value");
        var command = new Command("text", "Inject text input.");
        command.Arguments.Add(value);
        command.SetAction(parseResult => protocol.Text(parseResult.GetValue(value)));
        return command;
    }

    /// <summary>Creates the clipboard command.</summary>
    /// <param name="protocol">Command protocol that receives the parsed clipboard value.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateClipboardCommand(AgentWindowCommandProtocol protocol)
    {
        var value = AgentWindowCommandArguments.Words("value");
        var command = new Command("clipboard", "Set clipboard text.");
        command.Arguments.Add(value);
        command.SetAction(parseResult => protocol.Clipboard(parseResult.GetValue(value)));
        return command;
    }

    /// <summary>Creates a boolean state command.</summary>
    /// <param name="name">Command name.</param>
    /// <param name="description">Help description.</param>
    /// <param name="action">Protocol action that receives the parsed boolean.</param>
    /// <returns>The configured command.</returns>
    private static Command CreateBoolCommand(string name, string description, Action<bool> action)
    {
        var value = new Argument<bool>("value");
        var command = new Command(name, description);
        command.Arguments.Add(value);
        command.SetAction(parseResult => action(parseResult.GetValue(value)));
        return command;
    }
}
