
namespace GitInsight.Core;

public record RepositoryEntryDTO(string uri, string commit, Mode mode, List<Dictionary<string, object>> results);

public record RepositoryFindDTO(string uri, string commit, Mode mode);