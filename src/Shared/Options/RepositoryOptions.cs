namespace DefaultProjects.Shared.Options;

public record RepositoryOptions
{
    public static RepositoryOptions Default => _default;
    private static readonly RepositoryOptions _default = new();

    public bool UseAsTracking { get; set; }
    public bool EnableCaching { get; set; }
    public string[] IncludeRelatedEntities { get; set; }

    public RepositoryOptions(bool UseAsTracking = false, bool EnableCaching = false, IEnumerable<string>? IncludeRelatedEntities = null)
    {
        this.UseAsTracking = UseAsTracking;
        this.EnableCaching = EnableCaching;
        this.IncludeRelatedEntities = IncludeRelatedEntities?.ToArray() ?? Array.Empty<string>();
    }
}
