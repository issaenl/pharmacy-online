using System.Text.Json;

namespace pharmacyBackend.Services
{
    public class GeocodeService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;

        public GeocodeService(HttpClient httpClient, IConfiguration configuration)
        {
            _httpClient = httpClient;
            _configuration = configuration;
        }

        public async Task<(double? lat, double? lon)> GetCoordinatesAync(string address)
        {
            try
            {
                string apiKey = _configuration["Yandex:ApiKey"];
                string requestUrl = $"https://geocode-maps.yandex.ru/1.x/?apikey={apiKey}&geocode={Uri.EscapeDataString(address)}&format=json";
                var response = await _httpClient.GetStringAsync(requestUrl);
                var json = JsonDocument.Parse(response);
                var featureMember = json.RootElement
                    .GetProperty("response")
                    .GetProperty("GeoObjectCollection")
                    .GetProperty("featureMember");

                if(featureMember.GetArrayLength() == 0)
                {
                    return (null, null);
                }

                var pos = featureMember[0]
                    .GetProperty("GeoObject")
                    .GetProperty("Point")
                    .GetProperty("pos")
                    .GetString();

                if (string.IsNullOrEmpty(pos))
                {
                    return (null, null);
                }

                var parts = pos.Split(' ');
                var lon = double.Parse(parts[0], System.Globalization.CultureInfo.InvariantCulture);
                var lat = double.Parse(parts[1], System.Globalization.CultureInfo.InvariantCulture);

                return (lat, lon);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка: {ex.Message}");
                return (null, null);
            }
        }
    }
}
