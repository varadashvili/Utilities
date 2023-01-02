using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;

using Utils.Core.Classes;

namespace Utils.Core.Code
{
    public static class UtilitiesWebClientShared
    {
        private static HttpClientHandler clientHandlerWithoutValidation;
        private static HttpClientHandler clientHandlerWithValidation;

        private static HttpClient _httpClientWithValidation;
        private static HttpClient _httpClientWithoutValidation;

        private static HttpClient httpClientWithValidation
        {
            get
            {
                if (_httpClientWithValidation == null)
                {
                    ConfigureUtilities();
                }

                return _httpClientWithValidation;
            }

            set => _httpClientWithValidation = value;
        }
        private static HttpClient httpClientWithoutValidation
        {
            get
            {
                if (_httpClientWithoutValidation == null)
                {
                    ConfigureUtilities();
                }

                return _httpClientWithoutValidation;
            }

            set => _httpClientWithoutValidation = value;
        }


        private static void ConfigureUtilities()
        {
            if (_httpClientWithValidation == null)
            {
                clientHandlerWithValidation = new HttpClientHandler();
                _httpClientWithValidation = new HttpClient(handler: clientHandlerWithValidation, disposeHandler: true);
                _httpClientWithValidation.Timeout = TimeSpan.FromMinutes(30);
            }

            if (_httpClientWithoutValidation == null)
            {
                clientHandlerWithoutValidation = new HttpClientHandler
                {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) => { return true; }
                };

                _httpClientWithoutValidation = new HttpClient(handler: clientHandlerWithoutValidation, disposeHandler: true);
                _httpClientWithoutValidation.Timeout = TimeSpan.FromMinutes(30);
            }
        }

        public static void SetHttpClientTimeout(TimeSpan Timeout)
        {
            if (_httpClientWithValidation != null)
            {
                _httpClientWithValidation.Timeout = Timeout;
            }

            if (_httpClientWithoutValidation != null)
            {
                _httpClientWithoutValidation.Timeout = Timeout;
            }
        }


        //async requests returning stream response
        public static async Task<byte[]> ServiceRequestStreamAsync(string requestUrl, string content = null, RequestOptions requestOptions = null)
        {
            var httpClient = requestOptions?.DisableCertificateValidation == true ? httpClientWithoutValidation : httpClientWithValidation;

            var timeout = requestOptions?.RequestTimeout ?? TimeSpan.FromMinutes(30);

            using (var message = new HttpRequestMessage())
            {
                message.RequestUri = new Uri(requestUrl);

                if (requestOptions?.RequestHeaders?.Count > 0)
                {
                    foreach (var header in requestOptions.RequestHeaders)
                    {
                        if (header.Key.ToLower() != "content-type")
                        {
                            message.Headers.Add(header.Key, header.Value);
                        }
                    }
                }

                if (content.IsNotEmpty())
                {
                    message.Method = HttpMethod.Post;
                    message.Content = new StringContent(content);

                    if (requestOptions?.RequestHeaders?.Keys?.Any(x => x.ToLower() == "content-type") == true)
                    {
                        message.Content.Headers.ContentType = new MediaTypeHeaderValue(requestOptions.RequestHeaders["Content-Type"]);
                    }
                    else
                    {
                        message.Content.Headers.ContentType = new MediaTypeHeaderValue(@"application/json") { CharSet = Encoding.UTF8.WebName };
                    }
                }
                else
                {
                    message.Method = HttpMethod.Get;
                }

                if (requestOptions?.HttpMethod != null)
                {
                    message.Method = requestOptions.HttpMethod;
                }

                var responseMessage = await httpClient.SendAsync(message).TimeoutAfterAsync(timeout).ConfigureAwait(false);

                responseMessage.EnsureSuccessStatusCode();

                var responseContent = await responseMessage.Content.ReadAsByteArrayAsync().ConfigureAwait(false);

                return responseContent;
            }
        }

        public static Task<byte[]> ServiceRequestStreamAsync(string requestBaseUrl, Dictionary<string, object> uriParams, string content = null, RequestOptions requestOptions = null)
        {
            string url = requestBaseUrl.TrimEnd('&', '/');

            if (uriParams != null && uriParams.Count > 0)
            {
                url += "?" + uriParams.Aggregate(new StringBuilder(),
                    (sb, kvp) => sb.AppendFormat("{0}={1}&", kvp.Key, WebUtility.UrlEncode(kvp.Value.ToJson().Trim('"'))),
                    sb => sb.ToString());

                url = url.TrimEnd('&');
            }

            var responseTask = ServiceRequestStreamAsync(url, content, requestOptions);

            return responseTask;
        }

        public static Task<byte[]> ServiceRequestWebApiStreamAsync(string requestBaseUrl, string controllerName, string actionName, Dictionary<string, object> uriParams = null, string content = null, RequestOptions requestOptions = null)
        {
            string url = $"{requestBaseUrl.TrimEnd('&', '/')}/{controllerName}/{actionName}";

            var responseTask = ServiceRequestStreamAsync(url, uriParams, content, requestOptions);

            return responseTask;
        }


        //requests returning json response
        public static Task<string> ServiceRequestJsonAsync(string requestUrl, string content = null, RequestOptions requestOptions = null)
        {
            var responseTask = ServiceRequestJsonAsync(requestUrl, null, content, requestOptions);

            return responseTask;
        }

        public static async Task<string> ServiceRequestJsonAsync(string requestBaseUrl, Dictionary<string, object> uriParams, string content = null, RequestOptions requestOptions = null)
        {
            string jsonResponse = string.Empty;

            var contentArray = await ServiceRequestStreamAsync(requestBaseUrl, uriParams, content, requestOptions).ConfigureAwait(false);

            if (contentArray != null && contentArray.Length > 0)
            {
                jsonResponse = contentArray.BytesToString();
            }

            return jsonResponse;
        }

        public static Task<string> ServiceRequestWebApiJsonAsync(string requestBaseUrl, string controllerName, string actionName, Dictionary<string, object> uriParams = null, string content = null, RequestOptions requestOptions = null)
        {
            string url = $"{requestBaseUrl.TrimEnd('&', '/')}/{controllerName}/{actionName}";

            var responseTask = ServiceRequestJsonAsync(url, uriParams, content, requestOptions);

            return responseTask;
        }


        //requests returning Task<T> response
        public static Task<T> ServiceRequestAsync<T>(string requestUrl, string content = null, RequestOptions requestOptions = null)
        {
            var responseTask = ServiceRequestAsync<T>(requestUrl, null, content, requestOptions);

            return responseTask;
        }

        public static async Task<T> ServiceRequestAsync<T>(string requestBaseUrl, Dictionary<string, object> uriParams, string content = null, RequestOptions requestOptions = null)
        {
            T response = default(T);

            string jsonResponse = await ServiceRequestJsonAsync(requestBaseUrl, uriParams, content, requestOptions).ConfigureAwait(false);

            if (jsonResponse.IsEmpty())
            {
                throw new Exception("empty response!");
            }
            else
            {
                response = jsonResponse.FromJson<T>();

                if (response == null)
                {
                    throw new Exception("response json can't be casted!");
                }
            }

            return response;
        }

        public static Task<T> ServiceRequestWebApiAsync<T>(string requestBaseUrl, string controllerName, string actionName, Dictionary<string, object> uriParams = null, string content = null, RequestOptions requestOptions = null)
        {
            string url = $"{requestBaseUrl.TrimEnd('&', '/')}/{controllerName}/{actionName}";

            var responseTask = ServiceRequestAsync<T>(url, uriParams, content, requestOptions);

            return responseTask;
        }
    }
}
