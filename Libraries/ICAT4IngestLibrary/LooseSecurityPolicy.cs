using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace ICAT4IngestLibrary
{
    /// <summary>
    /// Class to deal with (ignore) any certificate issues
    /// </summary>
    public class LooseSecurityPolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint srvPoint, System.Security.Cryptography.X509Certificates.X509Certificate certificate, WebRequest request, int problem)
        {
            return problem == 0
                || problem == -2146762487  // CertUNTRUSTEDROOT
                || problem == -2146762481; // CertCN_NO_MATCH
        }
    }
}
