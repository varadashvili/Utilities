using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Xml.Serialization;

namespace Utils.Crypto.Code
{
    public static class UtilitiesCrypto
    {
        public static X509Certificate2 GetCertificateFromStore(string certSubject)
        {
            X509Certificate2 certificate = null;

            string subject = $"CN={certSubject}";

            // Access Personal (MY) certificate store of current user
            X509Store my = new X509Store(StoreName.My, StoreLocation.CurrentUser);

            my.Open(OpenFlags.ReadOnly);

            foreach (X509Certificate2 cert in my.Certificates)
            {
                if (cert.Subject.Contains(certSubject))
                {
                    //check for multiple.
                    if (certificate != null)
                    {
                        throw new Exception("multiple certificates found");
                    }

                    certificate = cert;
                }
            }

            return certificate;
        }


        public static string SignData(string text, string certSubject)
        {
            var signature = string.Empty;

            var certificate = GetCertificateFromStore(certSubject);

            var csp = (RSACryptoServiceProvider)certificate.PrivateKey;
            //var key = certificate.GetRSAPrivateKey();

            if (csp == null)
            {
                throw new Exception("private key not found for certificate");
            }


            // Hash the data
            SHA1Managed sha1 = new SHA1Managed();

            UnicodeEncoding encoding = new UnicodeEncoding();

            byte[] data = encoding.GetBytes(text);

            byte[] hash = sha1.ComputeHash(data);

            // Sign the hash
            var signedData = csp.SignHash(hash, CryptoConfig.MapNameToOID("SHA1"));

            string signatureStr = Convert.ToBase64String(signedData);

            return signature;
        }


        public static bool VerifyUsingStoreSearch(string text, string signature, string certSubject)
        {
            var certificate = GetCertificateFromStore(certSubject);

            var signatureBytes = Convert.FromBase64String(signature);

            bool isValid = Verify(text, signatureBytes, certificate);

            return isValid;
        }


        public static bool VerifyUsingCertPath(string text, string signature, string certPath)
        {
            var certificate = new X509Certificate2(certPath);

            var signatureBytes = Convert.FromBase64String(signature);

            bool isValid = Verify(text, signatureBytes, certificate);

            return isValid;
        }


        public static void VerifyUsingRequestCert(string text, string signature, object request)
        {
            // If we want to use the client cert in an ASP.NET app, we may use something like this instead:
            //X509Certificate2 certificate = new X509Certificate2(Request.ClientCertificate.Certificate);

            var signatureBytes = Convert.FromBase64String(signature);

            //bool isValid = Verify(text, signatureBytes, certificate);

            //return isValid;
        }


        public static bool Verify(string text, byte[] signature, X509Certificate2 certificate)
        {
            // Get its associated CSP and public key
            RSACryptoServiceProvider csp = (RSACryptoServiceProvider)certificate.PublicKey.Key;


            // Hash the data
            SHA1Managed sha1 = new SHA1Managed();

            UnicodeEncoding encoding = new UnicodeEncoding();

            byte[] data = encoding.GetBytes(text);

            byte[] hash = sha1.ComputeHash(data);


            // Verify the signature with the hash
            return csp.VerifyHash(hash, CryptoConfig.MapNameToOID("SHA1"), signature);
        }



        public static string GetKeyString(RSAParameters publicKey)
        {
            var sw = new StringWriter();
            var xs = new XmlSerializer(typeof(RSAParameters));
            xs.Serialize(sw, publicKey);
            string publicKeyString = sw.ToString();

            return publicKeyString;
        }


        public static void EncodeDecodeUsingCertificate()
        {
            string workingText = "some text string";
            string certSubject = "test.ge";

            var certificate = GetCertificateFromStore(certSubject);

            RSACryptoServiceProvider csp = (RSACryptoServiceProvider)certificate.PrivateKey;


            var publicKey = csp.ExportParameters(false);

            //encode
            var csp2 = new RSACryptoServiceProvider();
            csp2.ImportParameters(publicKey);

            var data = Encoding.Unicode.GetBytes(workingText);
            var cypher = csp2.Encrypt(data, false);

            string ecoded = Convert.ToBase64String(cypher);


            //decode
            //var privateKey = csp.ExportParameters(true);
            //csp = new RSACryptoServiceProvider();
            //csp.ImportParameters(privateKey);

            var dataBytes = Convert.FromBase64String(ecoded);
            var decripted = csp.Decrypt(dataBytes, false);
            var finalText = Encoding.Unicode.GetString(decripted);
        }


        public static void EcodeDecodeUsingKeys()
        {
            string workingText = "some text string";

            RSACryptoServiceProvider csp = new RSACryptoServiceProvider(2048);

            var privateKey = csp.ExportParameters(true);
            var publicKey = csp.ExportParameters(false);

            var publicKeyString = GetKeyString(publicKey);
            var privateKeyString = GetKeyString(privateKey);

            //encode
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(publicKey);

            var data = Encoding.Unicode.GetBytes(workingText);
            var cypher = csp.Encrypt(data, false);

            string ecoded = Convert.ToBase64String(cypher);


            //decode
            csp = new RSACryptoServiceProvider();
            csp.ImportParameters(privateKey);

            var dataBytes = Convert.FromBase64String(ecoded);
            var decripted = csp.Decrypt(dataBytes, false);
            var finalText = Encoding.Unicode.GetString(decripted);
        }
    }
}
