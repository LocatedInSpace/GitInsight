using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace GitInsight.Tests;

public class TestPersistence : IDisposable
{
    private readonly GitInsightContext _context;
    private readonly Repository _repo;

    public TestPersistence()
    {
        var optionsBuilder = new DbContextOptionsBuilder<GitInsightContext>();
        var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();
        optionsBuilder.UseSqlite(connection);
        _context = new GitInsightContext(optionsBuilder.Options);
        _repo = new Repository(_context);
    }
    
    public void Dispose()
    {
        _context.Dispose();
    }
    
    
    [Fact]
    public void Test_Save_Results()
    {
        // Arrange
        var repoName = "/path/to/repo";
        var commit = "d9cfd8f8b6d4c3b2a1a0a9";
        var results = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>("[{\"date\":\"2022-12-01\",\"count\":1},{\"date\":\"2022-12-02\",\"count\":1},{\"date\":\"2022-12-04\",\"count\":5},{\"date\":\"2022-12-05\",\"count\":2}]");
        var mode = Mode.CommitFrequency;

        // Act
        var response = _repo.Update(new RepositoryEntryDTO(repoName, commit, mode, results));

        // Assert
        Assert.Equal(Response.Created, response);

        var entry = _repo.Find(new RepositoryFindDTO(repoName, commit, mode));
        Assert.NotNull(entry);
        Assert.Equal(results, entry.results);
    }

    
    [Fact]
    public void Test_Update_Results()
    {
        // Arrange
        var repoName = "/path/to/repo";
        var commit = "d9cfd8f8b6d4c3b2a1a0a9";
        var results = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>("[{\"name\":\"LocatedInSpace\",\"totalCommits\":9,\"commits\":[{\"date\":\"2022-12-01\",\"count\":1},{\"date\":\"2022-12-02\",\"count\":1},{\"date\":\"2022-12-04\",\"count\":5},{\"date\":\"2022-12-05\",\"count\":2}]}]");
        var updatedCommit = "a9uer8f82jadc3b2a94dak";
        var updatedResults = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>("[{\"name\":\"LocatedInSpace\",\"totalCommits\":10,\"commits\":[{\"date\":\"2022-12-01\",\"count\":2},{\"date\":\"2022-12-02\",\"count\":1},{\"date\":\"2022-12-04\",\"count\":5},{\"date\":\"2022-12-05\",\"count\":2}]}]");
        var mode = Mode.CommitAuthor;
        
        _repo.Update(new RepositoryEntryDTO(repoName, commit, mode, results));
        
        // Act
        var response = _repo.Update(new RepositoryEntryDTO(repoName, updatedCommit, mode, updatedResults));

        // Assert
        Assert.Equal(Response.Updated, response);
        
        var entry = _repo.Find(new RepositoryFindDTO(repoName, commit, mode));
        Assert.Null(entry);
        
        entry = _repo.Find(new RepositoryFindDTO(repoName, updatedCommit, mode));
        Assert.Equal(updatedResults, entry.results);
    }
    
    [Fact]
    public void Test_Skip_Analysis()
    {
        // Arrange
        var repoName = "/path/to/repo";
        var commit = "d9cfd8f8b6d4c3b2a1a0a9";
        var results = System.Text.Json.JsonSerializer.Deserialize<List<Dictionary<string, object>>>("[{\"date\":\"2022-12-01\",\"count\":1},{\"date\":\"2022-12-02\",\"count\":1},{\"date\":\"2022-12-04\",\"count\":5},{\"date\":\"2022-12-05\",\"count\":2}]");
        var mode = Mode.CommitFrequency;

        var entry = new RepositoryEntryDTO(repoName, commit, mode, results);
        _repo.Update(entry);
        
        // Act
        var response = _repo.Find(new RepositoryFindDTO(repoName, commit, mode));

        // Assert
        response.Should().BeEquivalentTo(entry);
    }
}