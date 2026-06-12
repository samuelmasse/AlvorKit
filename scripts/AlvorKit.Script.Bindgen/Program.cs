using AlvorKit.Script.Bindgen;

var options = BindgenOptions.Parse(args);
var repository = RepositoryLayout.FindFrom(AppContext.BaseDirectory);
await new BindingGenerationRunner(repository, options).RunAsync();
