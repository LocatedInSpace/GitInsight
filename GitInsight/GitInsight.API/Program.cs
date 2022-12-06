using GitInsight.Core;
using GitInsight.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<GitInsightContext>(opt => opt.UseSqlite(
    "Data Source=D:/ETC/Projects/GitHub/GitInsight/GitInsight/GitInsight/bin/Debug/net6.0/git.db"));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();
/*
 if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
//*/

app.MapGet("/", () => "Hello World!");
app.MapGet("/get", GetAllRepos);
app.MapGet("/{name}/{project}", CloneOrUpdateRepo);

static async Task<IResult> GetAllRepos(GitInsightContext db)
{
    return Results.Ok(await db.Repositories.ToArrayAsync());
}

static async Task<IResult> CloneOrUpdateRepo(String name, String project, GitInsightContext db)
{
    var repo = new Repository(db);
    // mode hardcoded
    var result = repo.CloneOrPull($"{name}/{project}");
    if (result.Item1 == Response.Created)
    {
        return Results.Created("", result.Item2);
    }
    if (result.Item1 == Response.Updated)
    {
        return Results.Ok(result.Item2);
    }
    else
    {
        // internal server error
        return Results.StatusCode(500);
    }
    return Results.Ok(await db.Repositories.ToArrayAsync());
}

app.Run();
