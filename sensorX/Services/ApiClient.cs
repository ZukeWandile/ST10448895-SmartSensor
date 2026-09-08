using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace sensorX.Services
{
    //  helper class whose only job is to ask the API if is alive
    public class ApiClient
    {
        private readonly HttpClient _http; // reused HTTP client for making requests
        private readonly string _baseUrl;  

        public ApiClient(string baseUrl)
        {
            _baseUrl = baseUrl.TrimEnd('/'); // remove trailing slash so  don't get "//Scanner"
            _http = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(3) // don't hang the UI for long if API is offline
            };
        }

        // Returns true if the API responds successfully, false if it's offline or errors out
        public async Task<bool> IsApiOnlineAsync()
        {
            try
            {
                var response = await _http.GetAsync($"{_baseUrl}/Scanner");
                return response.IsSuccessStatusCode; // true for 200-299 status codes
            }
            catch
            {
                // Any exception (timeout, connection refused, etc.) means API is not reachable
                return false;
            }
        }
    }
}