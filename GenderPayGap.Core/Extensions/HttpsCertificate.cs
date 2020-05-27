using System;
using System.IO;
using System.Security.Cryptography.X509Certificates;

namespace GenderPayGap.Extensions
{
    public static class HttpsCertificate
    {

        public static X509Certificate2 LoadCertificateFromThumbprint(string certThumprint)
        {
            if (string.IsNullOrWhiteSpace(certThumprint))
            {
                throw new ArgumentNullException(nameof(certThumprint));
            }

            X509Certificate2 cert = null;
            using (var certStore = new X509Store(StoreName.My, StoreLocation.CurrentUser))
            {
                certStore.Open(OpenFlags.ReadOnly);

                //Try and get a valid cert
                X509Certificate2Collection certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, certThumprint, true);
                //Otherwise use an invalid cert
                if (certCollection.Count == 0)
                {
                    certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, certThumprint, false);
                }

                if (certCollection.Count > 0)
                {
                    cert = certCollection[0];
                }

                certStore.Close();
            }

            //Try again from local machine certificate store
            if (cert == null)
            {
                using (var certStore = new X509Store(StoreLocation.LocalMachine))
                {
                    certStore.Open(OpenFlags.ReadOnly);

                    //Try and get a valid cert
                    X509Certificate2Collection certCollection = certStore.Certificates.Find(
                        X509FindType.FindByThumbprint,
                        certThumprint,
                        true);
                    //Otherwise use an invalid cert
                    if (certCollection.Count == 0)
                    {
                        certCollection = certStore.Certificates.Find(X509FindType.FindByThumbprint, certThumprint, false);
                    }

                    if (certCollection.Count > 0)
                    {
                        cert = certCollection[0];
                    }

                    certStore.Close();
                }
            }

            if (cert == null)
            {
                throw new Exception($"Cannot find certificate with thumbprint '{certThumprint}' in local store");
            }

            return cert;
        }

        public static X509Certificate2 LoadCertificateFromFile(string certPath, string password)
        {
            if (string.IsNullOrWhiteSpace(certPath))
            {
                throw new ArgumentNullException(nameof(certPath));
            }

            if (!File.Exists(certPath))
            {
                throw new Exception($"Cannot find local certificate '{certPath}'");
            }

            var cert = new X509Certificate2(certPath, password, X509KeyStorageFlags.MachineKeySet);
            if (cert == null)
            {
                throw new Exception($"Cannot load certificate from file '{certPath}'");
            }

            return cert;
        }

    }
}
