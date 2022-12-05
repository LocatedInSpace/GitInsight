using GitInsight.Core;

namespace GitInsight.Infrastructure;

public class Repository : IRepository
{
    private readonly GitInsightContext _context;

    public Repository(GitInsightContext context)
    {
        _context = context;
    }
    
    public Response Update(RepositoryEntryDTO entry)
    {
        var result = _context.Repositories.FirstOrDefault(r => r.URI == entry.uri &&
                                                               r.Mode == entry.mode);
        if (result is null)
        {
            var e = new RepositoryEntry();
            e.URI = entry.uri;
            e.Commit = entry.commit;
            e.Mode = entry.mode;
            e.Results = entry.results;
            _context.Add(e);
            _context.SaveChanges();
            return Response.Created;
        }

        result.Commit = entry.commit;
        result.Results = entry.results;
        _context.SaveChanges();
        return Response.Updated;
    }

    public RepositoryEntryDTO? Find(RepositoryFindDTO info)
    {
        // head.tip is latest commit, sha is the hash
        var result = _context.Repositories.FirstOrDefault(r => r.URI == info.uri &&
                                                  r.Commit == info.commit &&
                                                  r.Mode == info.mode);
        // the stored commit is equal to the latest one - so reuse analysis
        if (result is not null)
        {
            return new RepositoryEntryDTO(result.URI, result.Commit, result.Mode, result.Results);
        }

        return null;
    }
}