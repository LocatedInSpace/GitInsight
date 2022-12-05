namespace GitInsight.Infrastructure;

public class GitInsightContextFactory : IDesignTimeDbContextFactory<GitInsightContext>
{
    public GitInsightContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<GitInsightContext>();
        optionsBuilder.UseSqlite("Data Source=git.db");

        return new GitInsightContext(optionsBuilder.Options);
    }
    
    public GitInsightContext CreateDbContext()
    {
        var optionsBuilder = new DbContextOptionsBuilder<GitInsightContext>();
        //optionsBuilder.UseSqlite("Data Source=git.db");
        optionsBuilder.UseSqlite(
            "Data Source=D:/ETC/Projects/GitHub/GitInsight/GitInsight/GitInsight/bin/Debug/net6.0/git.db");

        return new GitInsightContext(optionsBuilder.Options);
    }
}