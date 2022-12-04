using System;
using System.Collections.Generic;
using System.Linq;
using LibGit2Sharp;

namespace GitInsight
{
    enum Mode
    {
        CommitFrequency,
        CommitAuthor,
        Unknown
    }

    class Program
    {
        static void Main(string[] args)
        {
            // Parse the command-line arguments
            var mode = ParseArgs(args);

            // Get the path to the Git repository
            var path = args[0];

            // Open the repository
            var repo = new Repository(path);

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
                        Console.WriteLine($"{group.Count()} {group.Key:yyyy-MM-dd}");
                    }

                    break;

                case Mode.CommitAuthor:
                    // group all commits by author
                    var commitsByAuthorAndDate = repo.Commits
                        .GroupBy(c => c.Author.Name);

                    // show stats for each author
                    foreach (var group in commitsByAuthorAndDate)
                    {
                        
                        Console.WriteLine($"{group.Key} | {group.Count()} commits");
                        
                        // sort our authors commits into a grouping by date
                        var authorSorted = group.AsEnumerable()
                            .GroupBy(c => c.Author.When.Date)
                            .OrderBy(g => g.Key);
                        
                        foreach (var grouping in authorSorted)
                        {
                            Console.WriteLine($"      {grouping.Count()} {grouping.Key:yyyy-MM-dd}");
                        }
                    }

                    break;

                default:
                    Console.Error.WriteLine("Invalid mode");
                    break;
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