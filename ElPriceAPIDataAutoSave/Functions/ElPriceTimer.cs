using System;
using ElPriceAPIDataAutoSave.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Functions.Worker.Configuration;

namespace ElPriceAPIDataAutoSave.Functions;

public class ElPriceTimer
{
    private readonly ElPriceService _priceService;
    private readonly IElPriceHistoryService _history;
    private readonly ILogger<ElPriceTimer> _logger;

    public ElPriceTimer(
        ElPriceService priceService,
        IElPriceHistoryService history,
        ILogger<ElPriceTimer> logger)
    {
        _priceService = priceService;
        _history = history;
        _logger = logger;
    }

    [Function("ElPriceTimer")]
    public async Task Run([TimerTrigger("0 0 * * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("Timer triggered: Fetching electricity prices...");
        var prices = await _priceService.GetPricesAsync();
        await _history.AddSnapshotAsync(prices);

        _logger.LogInformation("Snapshot saved.");
    }
}
