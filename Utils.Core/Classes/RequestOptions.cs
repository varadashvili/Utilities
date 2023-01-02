using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace Utils.Core.Classes
{
    public class RequestOptions
    {
        public HttpMethod HttpMethod { get; set; }

        public Dictionary<string, string> RequestHeaders { get; set; }

        public bool? DisableCertificateValidation { get; set; }

        public TimeSpan? RequestTimeout { get; set; }
    }
}
