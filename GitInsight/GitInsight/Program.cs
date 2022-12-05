using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using LibGit2Sharp;
using GitRepository = LibGit2Sharp.Repository;
using GitInsight.Core;
using Mode = GitInsight.Core.Mode;
using GitInsight.Infrastructure;
using Repository = GitInsight.Infrastructure.Repository;


namespace GitInsight
{
    class Program
    {
        static void Main(string[] args)
        {
            // Parse the command-line arguments
            var mode = ParseArgs(args);

            // Get the path to the Git repository
            var path = args[0];

            // this is used for the database, we dont want to
            // use a relative path
            var uri = new Uri(path);

            // initialize context to/of database
            
            Console.WriteLine(GetResults(uri.AbsolutePath, mode));
        }

        private static string GetResults(string uri, Mode mode)
        {
            // Open the repository
            var repo = new GitRepository(uri);
            
            using var context = new GitInsightContextFactory().CreateDbContext();
            {
                var repository = new Repository(context);
                var result = repository.Find(new RepositoryFindDTO(uri, repo.Head.Tip.Sha, mode));
                //var result = context.Repositories.FirstOrDefault(r => r.URI == uri && r.Mode == mode);
                if (result is not null)
                {
                    Console.WriteLine("Retrieving from database");
                    return result.results;
                }

                StringBuilder sb = new StringBuilder();
                switch (mode)
                {
                    case Mode.CommitFrequency:
                        // group commits by their date, and then order by their grouped date
                        var commitsByDate = repo.Commits
                            .GroupBy(c => c.Author.When.Date)
                            .OrderBy(g => g.Key);

                        // Print the number of commits per day
                        foreach (var group in commitsByDate)
                        {
                            sb.AppendLine($"{group.Count()} {group.Key:yyyy-MM-dd}");
                        }

                        break;

                    case Mode.CommitAuthor:
                        // group all commits by author
                        var commitsByAuthorAndDate = repo.Commits
                            .GroupBy(c => c.Author.Name);

                        // show stats for each author
                        foreach (var group in commitsByAuthorAndDate)
                        {

                            sb.AppendLine($"{group.Key} | {group.Count()} commits");

                            // sort our authors commits into a grouping by date
                            var authorSorted = group.AsEnumerable()
                                .GroupBy(c => c.Author.When.Date)
                                .OrderBy(g => g.Key);

                            foreach (var grouping in authorSorted)
                            {
                                sb.AppendLine($"      {grouping.Count()} {grouping.Key:yyyy-MM-dd}");
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
                    var entry = new RepositoryEntryDTO(uri, repo.Head.Tip.Sha, mode, sb.ToString());
                    // ignore response
                    Console.WriteLine(repository.Update(entry));
                    return entry.results;
                }

                return "";
            }
        }

        // Parse the command-line arguments and return the mode
        static Mode ParseArgs(string[] args)
        {
            if (args.Length < 1)
            {
                Console.Error.WriteLine("Missing repository path");
                Environment.Exit(1);
            }

            if (args.Length < 2)
            {
                return Mode.CommitFrequency;
            }

            switch (args[1])
            {
                case "-a":
                case "--author":
                    return Mode.CommitAuthor;
                default:
                    return Mode.Unknown;
            }
        }
    }
}