using System.Security.Cryptography;
using System.Text;
using GitInsight.Core;
using IRepository = GitInsight.Core.IRepository;
using LibGit2Sharp;
using GitRepository = LibGit2Sharp.Repository;
using Mode = GitInsight.Core.Mode;


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

    public (Response, RepositoryJsonDTO?) CloneOrPull(string name)
    {
        // path to a folder in %temp%/*hex version of string*
        // this is to get around "/" in file names
        var result = Response.Updated;
        var path = Path.Combine(Path.GetTempPath(), Convert.ToHexString(Encoding.UTF8.GetBytes(name)));
        if (!Directory.Exists(path))
        {
            result = Response.Created;
            Directory.CreateDirectory(path);
            GitRepository.Clone($"https://github.com/{name}.git", path);
        }
        else
        {
            // handle exception
            using (var repo = new GitRepository(path))
            {
                Commands.Pull(repo, new Signature("guest", "guest", DateTimeOffset.Now), new PullOptions());
            }
        }

        using (var repo = new GitRepository(path))
        {
            return (result, new RepositoryJsonDTO(Analyze(repo, name, Mode.CommitFrequency)));
        }
        
        return (result, null);
    }

    // returns a json string
    private string Analyze(GitRepository repo, String name, Mode mode)
    {
        // Find looks for *exact* match - mode & last commit
        var exists = Find(new RepositoryFindDTO(name, repo.Head.Tip.Sha, mode));
        if (exists is not null)
        {
            Console.WriteLine("Retrieving from database");
            return exists.results;
        }

        var result = "";
        switch (mode)
        {
            case Mode.CommitFrequency:
                // group commits by their date, and then order by their grouped date
                var commitsByDate = repo.Commits
                    .GroupBy(c => c.Author.When.Date)
                    .OrderBy(g => g.Key);
                
                // Create a list of dictionaries, where each dictionary represents
                // a group of commits with the same date
                var commitsList = new List<Dictionary<string, object>>();
                
                foreach (var group in commitsByDate)
                {
                    var commitsOnDate = new Dictionary<string, object>();
                    commitsOnDate["date"] = group.Key.ToString("yyyy-MM-dd");
                    commitsOnDate["count"] = group.Count();
                    commitsList.Add(commitsOnDate);
                }
                result = System.Text.Json.JsonSerializer.Serialize(commitsList);
                break;

            case Mode.CommitAuthor:
                // group all commits by author
                var commitsByAuthorAndDate = repo.Commits
                    .GroupBy(c => c.Author.Name);

                // create a list of dictionaries, where each dictionary represents
                // a group of commits by a single author
                var authorsList = new List<Dictionary<string, object>>();
                foreach (var group in commitsByAuthorAndDate)
                {
                    var authorCommits = new Dictionary<string, object>();
                    authorCommits["name"] = group.Key;
                    authorCommits["totalCommits"] = group.Count();
                    
                    // create a list of dictionaries, where each dictionary represents
                    // a group of commits with the same date
                    var commitsListAuthor = new List<Dictionary<string, object>>();
                    var authorSorted = group.AsEnumerable()
                        .GroupBy(c => c.Author.When.Date)
                        .OrderBy(g => g.Key);
                    foreach (var grouping in authorSorted)
                    {
                        var commitsOnDate = new Dictionary<string, object>();
                        commitsOnDate["date"] = grouping.Key.ToString("yyyy-MM-dd");
                        commitsOnDate["count"] = grouping.Count();
                        commitsListAuthor.Add(commitsOnDate);
                    }
                }

                break;

            default:
                Console.Error.WriteLine("Invalid mode");
                break;
        }

        if (mode != Mode.Unknown)
        {
            Console.WriteLine("Saving to database");
            var entry = new RepositoryEntryDTO(name, repo.Head.Tip.Sha, mode, result);
            // ignore response
            Update(entry);
        }

        return result;
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