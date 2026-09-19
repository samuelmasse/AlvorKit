namespace AlvorKit;

/// <summary>Marks an import failure whose unknown path prevents complete dependency observation.</summary>
internal class SolutionImportException(Exception inner) : InvalidOperationException(
    $"Cannot establish import watches. Fix the import and restart the watcher: {inner.Message}", inner);
