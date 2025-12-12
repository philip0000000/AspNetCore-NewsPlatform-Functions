using System.Net.Http.Json;

namespace ElPriceAPIDataAutoSave.Services
{
    public class ElPriceService
    {
        private readonly HttpClient _httpClient;
        public ElPriceService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<ElPriceViewModel> GetPricesAsync()
        {
            // The public API endpoint for retrieving posts
            var url = "https://spotprices.lexlink.se/espot";
            // Make an HTTP GET request and parse the data into a list of ElPriceViewModel objects
            var elPrices = await _httpClient.GetFromJsonAsync<ElPriceViewModel>(url);
            return elPrices ?? new ElPriceViewModel();
        }
    }
}