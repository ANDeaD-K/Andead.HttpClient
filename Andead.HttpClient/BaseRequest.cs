using Newtonsoft.Json;
using System.Net.Http;

namespace Andead.HttpClient
{
    public abstract class BaseRequest
    {
        [JsonIgnore]
        public abstract string Path { get; }

        [JsonIgnore]
        public abstract HttpMethod HttpMethod { get; }
    }
}
