namespace GitInsight.Core;

public record RepositoryEntryDTO(string uri, string commit, Mode mode, string results);

public record RepositoryFindDTO(string uri, string commit, Mode mode);