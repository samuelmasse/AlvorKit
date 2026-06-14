using AlvorKit.Script.Bindgen;

var options = BindgenOptions.Parse(args);
var repository = RepositoryLayout.FindFrom(AppContext.BaseDirectory);
INativeLibrarySpec[] libraries =
[
    new FreeTypeLibrarySpec(),
    new GlfwLibrarySpec(),
    new MiniAudioLibrarySpec(),
    new OpenGlLibrarySpec(),
    new XxHashLibrarySpec()
];
await new BindingGenerationRunner(repository, options, libraries).RunAsync();
