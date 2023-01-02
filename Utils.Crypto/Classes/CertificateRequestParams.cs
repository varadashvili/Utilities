namespace Utils.Crypto.Classes
{
    internal class CertificateRequestParams
    {
        public string subjectName { get; set; }

        public CertificateType certType { get; set; }

        public int strength { get; set; }

        public int yearCount { get; set; }

        public string outputFileName { get; set; }

        public string exportPassword { get; set; }

        public CertificateRequestParams caCertificateParams { get; set; }
    }
}