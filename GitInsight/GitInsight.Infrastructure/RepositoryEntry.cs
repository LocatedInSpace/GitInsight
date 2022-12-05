using GitInsight.Core;

namespace GitInsight.Infrastructure;

public class RepositoryEntry
{
    public string URI { get; set; }
    public Mode Mode { get; set; }
    public string Commit { get; set; }
    public string Results { get; set; }
}