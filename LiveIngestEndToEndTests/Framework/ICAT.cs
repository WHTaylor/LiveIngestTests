using ICAT4IngestLibrary;
using System.ServiceModel;
using System.Configuration;

namespace LiveIngestEndToEndTests.Framework
{
    public static class ICAT
    {
        public static ICATClient Client { get; private set; }

        public static void ConnectClient()
        {
            Client = new(
                ConfigurationManager.AppSettings["ICATUsername"],
                ConfigurationManager.AppSettings["ICATPassword"],
                new BasicHttpBinding(BasicHttpSecurityMode.Transport),
                new EndpointAddress("https://icat-dev.isis.stfc.ac.uk/ICATService/ICAT?wsdl"));
        }
    }
}
