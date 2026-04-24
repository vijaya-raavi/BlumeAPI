using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net;
using System.Text;

namespace Ontec.Core.MasterApi.GenericApi
{
    public abstract class GenericApiService
    {
        private readonly ILogger _logger;

        protected GenericApiService(ILogger logger)
        {
            _logger = logger;
        }
        protected async Task<TOut> MasterGetAsync<TIn, TOut>(string url)
        {
            try
            {
                var httpClientHandler = new HttpClientHandler()
                {
                    AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip
                };
                var httpClient = new HttpClient(httpClientHandler);
                httpClient.DefaultRequestHeaders.Add("Accept", "*/*");
                httpClient.DefaultRequestHeaders.Add("Accept-Encoding", "gzip, deflate, br");
                httpClient.DefaultRequestHeaders.Add("Connection", "keep-alive");
                //httpClient.DefaultRequestHeaders.Add("content-type", "application/gzip");
                _logger.LogInformation("IN Generic API to hit");
                _logger.LogInformation(url.ToString());
                var response = await httpClient.GetAsync(url);
                _logger.LogInformation("My Response here : " + JsonConvert.SerializeObject(response));
                response.EnsureSuccessStatusCode();
                _logger.LogInformation(response.EnsureSuccessStatusCode().ToString());
                var responseBody = await response.Content.ReadAsStringAsync();
                _logger.LogInformation(responseBody.ToString());
                //var settings = new JsonSerializerSettings
                //{
                //    DateFormatString = "yyyy-MM-ddTHH:mm:ssZ",
                //    DateTimeZoneHandling = DateTimeZoneHandling.Utc // Assuming dates are in UTC
                //};
               

                if (string.IsNullOrEmpty(responseBody))
                {
                    return default;
                }
                return JsonConvert.DeserializeObject<TOut>(responseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError("In catch");
                _logger.LogError(ex.ToString());
                return default(TOut);
            }
        }
        protected async Task<TOut> MasterPostAsync<TIn, TOut>(string url, TIn content)
        {
            var httpClient = new HttpClient();

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json")
            };
            request.Headers.Add("Accept", "*/*");
            request.Headers.Add("Accept-Encoding", "gzip, deflate,br");
            request.Headers.Add("connection", "keep-alive");
            var response = await httpClient.SendAsync(request);

            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            if (string.IsNullOrEmpty(responseBody))
            {
                return default;
            }
            return JsonConvert.DeserializeObject<TOut>(responseBody);
        }
    }
}