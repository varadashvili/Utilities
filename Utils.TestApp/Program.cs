using Utils.Core.Code;
using Utils.Crypto.Classes;
using Utils.Crypto.Code;

namespace Utils.TestApp;

internal class Program
{
    static void Main(string[] args)
    {
        var txt = UtilitiesWebRequestShared.ServiceRequestJson("https://google.com");

        var certificateParams = new CertificateParams
        {
            CertificatePassword = "123456",
            OutputFilePath = "c:"
        };
        UtilitiesCertificate.createSelfSigned(certificateParams);
    }
}