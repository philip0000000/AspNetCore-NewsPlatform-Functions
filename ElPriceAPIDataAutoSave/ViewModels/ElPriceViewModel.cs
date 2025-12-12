using System.Text.Json.Serialization;

public class ElPriceViewModel
{
    public string? Date { get; set; }

    [JsonPropertyName("SE1")]
    public SEItem[] SE1 { get; set; } = Array.Empty<SEItem>();

    [JsonPropertyName("SE2")]
    public SEItem[] SE2 { get; set; } = Array.Empty<SEItem>();

    [JsonPropertyName("SE3")]
    public SEItem[] SE3 { get; set; } = Array.Empty<SEItem>();

    [JsonPropertyName("SE4")]
    public SEItem[] SE4 { get; set; } = Array.Empty<SEItem>();

    public class SEItem
    {
        public int hour { get; set; }
        public float price_eur { get; set; }
        public float price_sek { get; set; }
        public int kmeans { get; set; }
    }
}
