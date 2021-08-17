using System.Reflection;
using System.Timers;
using System.Xml.Serialization;
using log4net;
using org.icatproject.isisicat.ICAT;

namespace ICAT4IngestLibrary
{
    /// <summary>
    /// Wraps an ICAT service connection to simplify interacting with the ICAT API.
    /// Creating an instance connects to ICAT and logs in the user whose credentials are provided.
    /// The service methods can then be accessed using the Service and SessionId properties.
    /// Automatically refreshes the session every ten minutes.
    ///
    /// TODO: Create wrappers for web methods to hide query strings and the object/entityBaseBean return types.
    /// </summary>
    public class ICATClient
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(ICATClient));

        public CATClient Service { get; }
        public string SessionId { get; }

        private Timer refreshTimer;
        private readonly int refreshDurationMinutes = 10;

        public ICATClient(string username, string password) : this("uows", username, password) { }
        public ICATClient(string authPlugin, string username, string password)
        {
            MethodInfo method = typeof(XmlSerializer).GetMethod(
                "set_Mode", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            method.Invoke(null, new object[] { 1 });
            Service = new CATClient();
            SessionId = Service.login(authPlugin, BuildCredentials(username, password));
            Logger.Debug($"Logged into ICAT as {username}");

            refreshTimer = new Timer(refreshDurationMinutes * 60 * 1000);
            refreshTimer.Elapsed += (s, e) => Service.refresh(SessionId);
            refreshTimer.Start();
        }

        private loginEntry[] BuildCredentials(string username, string password)
        {
            var user = new loginEntry()
            {
                key = "username",
                value = username
            };

            var pass = new loginEntry()
            {
                key = "password",
                value = password
            };
            return new loginEntry[] { user, pass };
        }
    }
}
