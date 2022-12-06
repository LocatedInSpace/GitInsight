using GitInsight.Core;

namespace GitInsight.Infrastructure;

public class RepositoryEntry
{
    public string URI { get; set; }
    public Mode Mode { get; set; }
    public string Commit { get; set; }
    public List<Dictionary<string, object>> Results { get; set; }
}