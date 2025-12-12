using AFHPNewsFunctions.Entities;
using AFHPNewsFunctions.Services;
using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace AFHPNewsFunctions.Functions;

public class CurrencyHistoryFunction
{
    private const string BASECURRENCY = "SEK";
    private List<string> CURRENCIES = new List<string> { "EUR", "USD", "GBP", "JPY", "CNY" };

    private readonly ILogger _logger;
    private readonly ICurrencyService _currencyService;
    private readonly ITableStorageService _tableStorageService;

    public CurrencyHistoryFunction(
        ILoggerFactory loggerFactory,
        ICurrencyService currencyService,
        ITableStorageService tableStorageService
    )
    {
        _logger = loggerFactory.CreateLogger<CurrencyHistoryFunction>();
        _currencyService = currencyService;
        _tableStorageService = tableStorageService;
    }

    [Function("CurrencyHistory")]
    public async Task Run([TimerTrigger("0 0 23 * * *")] TimerInfo myTimer)
    {
        _logger.LogInformation("Timer trigger CurrencyHistory Function started at: {executionTime}", DateTime.Now);

        try
        {
            // await SeedData();
            // return;

            var result = await _currencyService.Convert(BASECURRENCY, CURRENCIES, 1.0);
            if (result == null)
            {
                _logger.LogInformation("CurrencyHistory Function failed to fetch currency data");
                return;
            }

            var rowKey = result.Date; // Values update around 16 CET every day, until then it will return yesterdays values

            List<ITableEntity> entities = result.Rates.Select(kvp => (ITableEntity) new CurrencyEntity
            {
                PartitionKey = $"{BASECURRENCY}-{kvp.Key}",
                RowKey = rowKey,

                Value = kvp.Value
            })
            .ToList();

            var success = await _tableStorageService.Add(entities);
            if (!success)
            {
                _logger.LogWarning("CurrencyHistory Function did not save all currency data");
                return;
            }

            _logger.LogInformation("CurrencyHistory Function completed successfully at: {executionTime}", DateTime.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in CurrencyHistory Function {message}", ex.Message);

            // Try send email to admin?
            // await _emailSender.SendEmailAsync("admin.afhp.news@gmail.com", ...);

            // Re-throw so Azure marks the run as failed?
            // throw;
        }

        if (myTimer.ScheduleStatus is not null)
            _logger.LogInformation("CurrencyHistory Function Next timer schedule at: {nextSchedule}", myTimer.ScheduleStatus.Next);
    }

    private async Task SeedData()
    {
        //var startDate = "2025-11-01";
        //var endDate = "2025-12-03";

        var startDate = "2025-09-27";
        var endDate = "2025-10-30";

        var result = await _currencyService.ConvertRange(BASECURRENCY, CURRENCIES, 1.0, startDate, endDate);
        if (result != null)
        {
            foreach (var code in CURRENCIES) {
                var previousRates = result.Rates["2025-09-26"];

                var entities = new List<ITableEntity>();

                var sd = new DateTime(2025, 09, 27);
                var ed = new DateTime(2025, 10, 30);
                var cd = sd;
                // foreach (var date in result.Rates.Keys)
                while (cd <= ed)
                {
                    var date = cd.ToString("yyyy-MM-dd");

                    var rates = result.Rates.GetValueOrDefault(date);
                    if (rates == null)
                        rates = previousRates;
                    else
                        previousRates = rates;

                    if (rates != null)
                    {
                        var amount = rates.GetValueOrDefault(code);

                        var entity = (ITableEntity)new CurrencyEntity
                        {
                            PartitionKey = $"{BASECURRENCY}-{code}",
                            RowKey = date,

                            Value = amount
                        };

                        entities.Add(entity);

                        if (entities.Count == 100)
                        {
                            await _tableStorageService.AddBatch(entities);

                            entities = new List<ITableEntity>();
                        }
                    }

                    cd = cd.AddDays(1);
                }

                await _tableStorageService.AddBatch(entities);
            }
        }
        
        _logger.LogInformation("Added SeedData to Currency Table");
    }
}