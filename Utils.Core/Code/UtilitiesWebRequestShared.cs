using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net;
using System.Text;

using Utils.Core.Classes;

namespace Utils.Core.Code
{
    public static class UtilitiesWebRequestShared
    {
        private static RemoteCertificateValidationCallback disableValidCert = delegate { return true; };


        //requests returning stream response
        public static Stream ServiceRequestBaseStream(string requestUrl, string content = null, RequestOptions requestOptions = null)
        {
            var request = WebRequest.Create(requestUrl) as HttpWebRequest;
            request.AllowWriteStreamBuffering = false;
            request.AllowAutoRedirect = false;

            request.Timeout = (requestOptions?.RequestTimeout ?? TimeSpan.FromMinutes(30)).TotalMilliseconds.ConvertTo<int>();

            if (requestOptions?.DisableCertificateValidation == true)
            {
                request.ServerCertificateValidationCallback = disableValidCert;
            }

            if (requestOptions?.RequestHeaders?.Count > 0)
            {
                foreach (var header in requestOptions.RequestHeaders)
                {
                    if (header.Key.ToUpper() == "Content-Type".ToUpper())
                    {
                        request.ContentType = header.Value;
                    }
                    else
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            if (content.IsNotEmpty())
            {
                request.Method = "POST";

                if (!(requestOptions?.RequestHeaders?.Keys?.Any(x => x.ToLower() == "content-type") == true))
                {
                    request.ContentType = @"application/json; charset=utf-8";
                }

                request.ContentLength = Encoding.UTF8.GetBytes(content).Length;

                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(content);
                }
            }
            else
            {
                request.Method = "GET";
            }

            if (requestOptions?.HttpMethod != null)
            {
                request.Method = requestOptions.HttpMethod.ToString().ToUpper();
            }

            WebResponse response = request.GetResponse();
            var stream = response.GetResponseStream();

            return stream;
        }

        public static Stream ServiceRequestBaseStream(string requestBaseUrl, Dictionary<string, object> uriParams, string content = null, RequestOptions requestOptions = null)
        {
            string url = requestBaseUrl.TrimEnd('&', '/');

            if (uriParams != null && uriParams.Count > 0)
            {
                url += "?" + uriParams.Aggregate(new StringBuilder(),
                    (sb, kvp) => sb.AppendFormat("{0}={1}&", kvp.Key, WebUtility.UrlEncode(kvp.Value.ToJson().Trim('"'))),
                    sb => sb.ToString());

                url = url.TrimEnd('&');
            }

            var response = ServiceRequestBaseStream(url, content, requestOptions);

            return response;
        }

        public static Stream ServiceRequestWebApiBaseStream(string requestBaseUrl, string controllerName, string actionName, Dictionary<string, object> uriParams = null, string content = null, RequestOptions requestOptions = null)
        {
            string url = $"{requestBaseUrl.TrimEnd('&', '/')}/{controllerName}/{actionName}";

            var response = ServiceRequestBaseStream(url, uriParams, content, requestOptions);

            return response;
        }


        //requests returning byte[] response
        public static byte[] ServiceRequestStream(string requestUrl, string content = null, RequestOptions requestOptions = null)
        {
            byte[] responseBytes = null;

            var request = WebRequest.Create(requestUrl) as HttpWebRequest;
            request.AllowWriteStreamBuffering = false;
            request.AllowAutoRedirect = false;

            request.Timeout = (requestOptions?.RequestTimeout ?? TimeSpan.FromMinutes(30)).TotalMilliseconds.ConvertTo<int>();

            if (requestOptions?.DisableCertificateValidation == true)
            {
                request.ServerCertificateValidationCallback = disableValidCert;
            }

            if (requestOptions?.RequestHeaders?.Count > 0)
            {
                foreach (var header in requestOptions.RequestHeaders)
                {
                    if (header.Key.ToUpper() == "Content-Type".ToUpper())
                    {
                        request.ContentType = header.Value;
                    }
                    else
                    {
                        request.Headers.Add(header.Key, header.Value);
                    }
                }
            }

            if (content.IsNotEmpty())
            {
                request.Method = "POST";

                if (!(requestOptions?.RequestHeaders?.Keys?.Any(x => x.ToLower() == "content-type") == true))
                {
                    request.ContentType = @"application/json; charset=utf-8";
                }

                request.ContentLength = Encoding.UTF8.GetBytes(content).Length;

                using (StreamWriter writer = new StreamWriter(request.GetRequestStream()))
                {
                    writer.Write(content);
                }
            }
            else
            {
                request.Method = "GET";
            }

            if (requestOptions?.HttpMethod != null)
            {
                request.Method = requestOptions.HttpMethod.Method.ToUpper();
            }


            WebResponse response = request.GetResponse();
            var stream = response.GetResponseStream();

            if (stream != null)
            {
                using (var responseStream = new MemoryStream())
                {
                    stream.CopyTo(responseStream);

                    responseBytes = responseStream.ToArray();
                }
            }

            response.Close();
            request.Abort();

            return responseBytes;
        }

        public static byte[] ServiceRequestStream(string requestBaseUrl, Dictionary<string, object> uriParams, string content = null, RequestOptions requestOptions = null)
        {
            string url = requestBaseUrl.TrimEnd('&', '/');

            if (uriParams != null && uriParams.Count > 0)
            {
                url += "?" + uriParams.Aggregate(new StringBuilder(),
                    (sb, kvp) => sb.AppendFormat("{0}={1}&", kvp.Key, WebUtility.UrlEncode(kvp.Value.ToJson().Trim('"'))),
                    sb => sb.ToString());

                url = url.TrimEnd('&');
            }

            var response = ServiceRequestStream(url, content, requestOptions);

            return response;
        }

        public static byte[] ServiceRequestWebApiStream(string requestBaseUrl, string controllerName, string actionName, Dictionary<string, object> uriParams = null, string content = null, RequestOptions requestOptions = null)
        {
            string url = $"{requestBaseUrl.TrimEnd('&', '/')}/{controllerName}/{actionName}";

            var response = ServiceRequestStream(url, uriParams, content, requestOptions);

            return response;
        }


        //requests returning json response
        public static string ServiceRequestJson(string requestUrl, string content = null, RequestOptions requestOptions = null)
        {
            var jsonResponse = ServiceRequestJson(requestUrl, null, content, requestOptions);

            return jsonResponse;
        }

        public static string ServiceRequestJson(string requestBaseUrl, Dictionary<string, object> uriParams, string content = null, RequestOptions requestOptions = null)
        {
            string jsonResponse = string.Empty;

            var responseArray = ServiceRequestStream(requestBaseUrl, uriParams, content, requestOptions);

            if (responseArray != null && responseArray.Length > 0)
            {
                jsonResponse = responseArray.BytesToString();
            }

            return jsonResponse;
        }

        public static string ServiceRequestWebApiJson(string requestBaseUrl, string controllerName, string actionName, Dictionary<string, object> uriParams = null, string content = null, RequestOptions requestOptions = null)
        {
            string url = $"{requestBaseUrl.TrimEnd('&', '/')}/{controllerName}/{actionName}";

            var jsonResponse = ServiceRequestJson(url, uriParams, content, requestOptions);

            return jsonResponse;
        }


        //requests returning T response
        public static T ServiceRequest<T>(string requestUrl, string content = null, RequestOptions requestOptions = null)
        {
            var response = ServiceRequest<T>(requestUrl, null, content, requestOptions);

            return response;
        }

        public static T ServiceRequest<T>(string requestBaseUrl, Dictionary<string, object> uriParams, string content = null, RequestOptions requestOptions = null)
        {
            T response = default(T);

            string jsonResponse = ServiceRequestJson(requestBaseUrl, uriParams, content, requestOptions);

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

        public static T ServiceRequestWebApi<T>(string requestBaseUrl, string controllerName, string actionName, Dictionary<string, object> uriParams = null, string content = null, RequestOptions requestOptions = null)
        {
            string url = $"{requestBaseUrl.TrimEnd('&', '/')}/{controllerName}/{actionName}";

            var response = ServiceRequest<T>(url, uriParams, content, requestOptions);

            return response;
        }
    }
}
