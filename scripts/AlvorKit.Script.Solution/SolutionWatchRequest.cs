namespace AlvorKit;

/// <summary>Identifies one repository's discovery or graph work; an empty root requests parent discovery.</summary>
internal record SolutionWatchRequest(string Root, bool Discovery);
