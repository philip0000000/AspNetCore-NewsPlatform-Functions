using ArchiveNewsFunction.Data;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace ArchiveNewsFunction.Functions;

public class ArchiveNews
{
    private readonly ILogger _logger;
    private readonly IDbContextFactory<ApplicationDbContext> _factory;

    public ArchiveNews(ILoggerFactory loggerFactory, IDbContextFactory<ApplicationDbContext> factory)
    {
        _logger = loggerFactory.CreateLogger<ArchiveNews>();
        _factory = factory;
    }

    [Function("ArchiveNews")]
    public async Task Run([TimerTrigger("0 0 3 * * *")] TimerInfo myTimer)    //("0 * * * * *") for test
    {
        _logger.LogInformation("Timer executed at: {time}", DateTime.Now);

        using var _db = _factory.CreateDbContext();

        var articles = await _db.Articles
            .Include(a => a.ArticleStatus)
            .Where(a => a.PublishedTime <= DateTime.UtcNow.AddDays(-30))
            .ToListAsync();

        if (!articles.Any())
        {
            _logger.LogInformation("No articles older than 30 days.");
            return;
        }

        foreach (var a in articles)
        {
            _logger.LogInformation("Old Article → {id} | {slug} | {published}",
                a.Id, a.Slug, a.PublishedTime);

            // change in memory
            a.ArticleStatusId = 5;
        }
        await _db.SaveChangesAsync();
    }
}