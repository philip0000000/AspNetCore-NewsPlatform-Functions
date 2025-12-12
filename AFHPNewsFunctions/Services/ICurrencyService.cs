
namespace AFHPNewsFunctions.Services
{
    public interface ICurrencyService
    {
        Task<ConvertCurrencyDTO?> Convert(string baseCurrency, List<string> currencies, double amount = 1);

        Task<ConvertCurrencyRangeDTO?> ConvertRange(string baseCurrency, List<string> currencies, double v, string startDate, string endDate);
    }
}
