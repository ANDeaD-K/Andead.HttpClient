using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace Andead.HttpClient
{
    public class Client<TBaseResponse> where TBaseResponse : BaseResponse
    {
        private readonly System.Net.Http.HttpClient _httpClient;
        private readonly JsonSerializerSettings _serializerSettings = new()
        {
            ContractResolver = new DefaultContractResolver
            {
                NamingStrategy = new SnakeCaseNamingStrategy()
            }
        };

        public Client(System.Net.Http.HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        private TBaseResponse ResponseBuilder(HttpResponseMessage message, JsonSerializerSettings serializerSettings)
        {
            return (TBaseResponse)Activator.CreateInstance(typeof(TBaseResponse), message, serializerSettings);
        }

        private readonly Func<object, JsonSerializerSettings, HttpContent> ContentBuilder = (request, serializerSettings) =>
        {
            var content = new StringContent(JsonConvert.SerializeObject(request, serializerSettings));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            return content;
        };

        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(TRequest request)
            where TRequest : BaseRequest
        {
            return await ExecuteAsync<TRequest, TResponse>(request, CancellationToken.None);
        }

        public async Task<TResponse> ExecuteAsync<TRequest, TResponse>(TRequest request, CancellationToken cancellationToken)
            where TRequest : BaseRequest
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var content = ContentBuilder(request, _serializerSettings);

            using (var message = new HttpRequestMessage())
            {
                message.Content = content;
                message.Method = request.HttpMethod;

                message.RequestUri = new Uri($"{_httpClient.BaseAddress.LocalPath}{request.Path}", UriKind.Relative);

                try
                {
                    var httpResponse = await _httpClient
                        .SendAsync(message, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                        .ConfigureAwait(false);

                    var response = ResponseBuilder(httpResponse, _serializerSettings);
                    return await response.GetContentOrThrow<TResponse>();
                }
                catch (Exception ex)
                {
                    throw new Exception($"{ex.Message} ({ex.InnerException?.Message ?? "null"})");
                }
            }
        }
    }
}
