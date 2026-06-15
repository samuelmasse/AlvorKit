namespace AlvorKit.Script.TestCoverage;

/// <summary>Basic project metadata used for coverage project discovery.</summary>
/// <param name="Path">Absolute project path.</param>
/// <param name="AssemblyName">Assembly name inferred from the project file name.</param>
internal sealed record ProjectInfo(string Path, string AssemblyName);
