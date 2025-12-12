using Azure;
using Azure.Data.Tables;
using System.Net.Http.Json;
using System.Text.Json;

namespace AFHPNewsFunctions.Services
{
    public class CurrencyService : ICurrencyService
    {
        private readonly HttpClient _httpClient;

        // Updated daily around 16:00 CET.
        private const string APIURL = "https://api.frankfurter.dev/v1/";
        public CurrencyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ConvertCurrencyDTO?> Convert(string baseCurrency, List<string> currencies, double amount = 1)
        {
            string url = APIURL + $"latest?base={baseCurrency}&symbols={string.Join(",", currencies)}";

            try
            {
                ConvertCurrencyDTO? result =
                    await _httpClient.GetFromJsonAsync<ConvertCurrencyDTO>(url, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result;
            }
            catch (HttpRequestException ex)
            {
                //errorResponse.error = "Network error";
            }
            catch (NotSupportedException ex)
            {
                //errorResponse.error = "Convertion error";
            }
            catch (JsonException ex)
            {
                //errorResponse.error = "Convertion error";
            }
            catch (Exception ex)
            {
                //errorResponse.error = ex.Message;
            }

            return null;
        }

        public async Task<ConvertCurrencyRangeDTO?> ConvertRange(string baseCurrency, List<string> currencies, double v, string startDate, string endDate)
        {
            string url = APIURL + $"{startDate}..{endDate}?base={baseCurrency}&symbols={string.Join(",", currencies)}";

            try
            {
                ConvertCurrencyRangeDTO? result =
                    await _httpClient.GetFromJsonAsync<ConvertCurrencyRangeDTO>(url, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return result;

            }
            catch (HttpRequestException ex)
            {
                // errorResponse.error = "Network error";
            }
            catch (NotSupportedException ex)
            {
                // errorResponse.error = "Convertion error";
            }
            catch (JsonException ex)
            {
                // errorResponse.error = "Convertion error";
            }
            catch (Exception ex)
            {
                // errorResponse.error = ex.Message;
            }

            return null;
        }

        private List<CurrencyInfo> GetAllCurrencyInfo()
        {
            var currencies = new List<CurrencyInfo>
            {
            new CurrencyInfo("AUD", "Australia", "Australian Dollar", "$", true),
            new CurrencyInfo("BGN", "Bulgaria", "Bulgarian Lev", "лв", false),
            new CurrencyInfo("BRL", "Brazil", "Brazilian Real", "R$", true),
            new CurrencyInfo("CAD", "Canada", "Canadian Dollar", "$", true),
            new CurrencyInfo("CHF", "Switzerland", "Swiss Franc", "CHF", true),
            new CurrencyInfo("CNY", "China", "Chinese Yuan", "¥", true),
            new CurrencyInfo("CZK", "Czech Republic", "Czech Koruna", "Kč", false),
            new CurrencyInfo("DKK", "Denmark", "Danish Krone", "kr", false),
            new CurrencyInfo("EUR", "European Union", "Euro", "€", false),
            new CurrencyInfo("GBP", "United Kingdom", "British Pound", "£", true),
            new CurrencyInfo("HKD", "Hong Kong", "Hong Kong Dollar", "$", true),
            new CurrencyInfo("HUF", "Hungary", "Hungarian Forint", "Ft", false),
            new CurrencyInfo("IDR", "Indonesia", "Indonesian Rupiah", "Rp", true),
            new CurrencyInfo("ILS", "Israel", "Israeli New Shekel", "₪", true),
            new CurrencyInfo("INR", "India", "Indian Rupee", "₹", true),
            new CurrencyInfo("ISK", "Iceland", "Icelandic Króna", "kr", false),
            new CurrencyInfo("JPY", "Japan", "Japanese Yen", "¥", true),
            new CurrencyInfo("KRW", "South Korea", "South Korean Won", "₩", true),
            new CurrencyInfo("MXN", "Mexico", "Mexican Peso", "$", true),
            new CurrencyInfo("MYR", "Malaysia", "Malaysian Ringgit", "RM", true),
            new CurrencyInfo("NOK", "Norway", "Norwegian Krone", "kr", false),
            new CurrencyInfo("NZD", "New Zealand", "New Zealand Dollar", "$", true),
            new CurrencyInfo("PHP", "Philippines", "Philippine Peso", "₱", true),
            new CurrencyInfo("PLN", "Poland", "Polish Złoty", "zł", false),
            new CurrencyInfo("RON", "Romania", "Romanian Leu", "lei", false),
            new CurrencyInfo("SEK", "Sweden", "Swedish Krona", "kr", false),
            new CurrencyInfo("SGD", "Singapore", "Singapore Dollar", "$", true),
            new CurrencyInfo("THB", "Thailand", "Thai Baht", "฿", true),
            new CurrencyInfo("TRY", "Turkey", "Turkish Lira", "₺", true),
            new CurrencyInfo("USD", "United States", "US Dollar", "$", true),
            new CurrencyInfo("ZAR", "South Africa", "South African Rand", "R", true)
            }.OrderBy(c => c.Country).ToList();

            return currencies;
        }
    }

    public class ConvertCurrencyDTO
    {
        public double Amount { get; set; }
        public string Base { get; set; } = string.Empty;
        public string Date { get; set; }
        public Dictionary<string, double> Rates { get; set; }
    }

    public class ConvertCurrencyRangeDTO
    {
        public double Amount { get; set; }
        public string Base { get; set; }
        public string Start_date { get; set; }
        public string End_date { get; set; }
        public Dictionary<string, Dictionary<string, double>> Rates { get; set; }
    }

    public class CurrencyInfo
    {
        public string Code { get; set; }
        public string Country { get; set; }
        public string Name { get; set; }
        public string Symbol { get; set; }
        public bool Before { get; set; }
        public double Amount { get; set; } = 0;

        public CurrencyInfo(string code, string country, string name, string symbol, bool before, double amount = 0)
        {
            Code = code;
            Country = country;
            Name = name;
            Symbol = symbol;
            Before = before; // Place Symbol before Amount
            Amount = amount;
        }

        public override string ToString() => $"{Country} - {Name} ({Symbol} {Code})";
        public string AmountToString()
        {
            if (Before)
                return $"{Symbol} {Amount.ToString("F2")}";
            return $"{Amount.ToString("F2")} {Symbol}";
        }

        public CurrencyInfo Clone(double amount) => new CurrencyInfo(Code, Country, Name, Symbol, Before, amount);
    }
}
