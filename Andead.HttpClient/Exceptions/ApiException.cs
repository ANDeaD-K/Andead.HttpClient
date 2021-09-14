using System;
using System.Net;

namespace Andead.HttpClient.Exceptions
{
    public class ApiException : Exception
    {
        public HttpStatusCode StatusCode { get; }

        public string Response { get; }

        public ApiException(string message, HttpStatusCode statusCode, string response, Exception innerException)
            : base(message, innerException)
        {
            StatusCode = statusCode;
            Response = $"{message}, Status: {statusCode}, Response: {(response ?? "(null)")}";
        }
    }
}