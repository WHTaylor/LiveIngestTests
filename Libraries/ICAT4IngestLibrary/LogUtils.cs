using System.IO;
using System.Linq;
using log4net;
using log4net.Appender;
using log4net.Repository.Hierarchy;

namespace ICAT4IngestLibrary
{
    public static class LogUtils
    {
        public static void UseTestLogFile()
        {
            var appender = (LogManager.GetRepository() as Hierarchy)
                ?.GetAppenders()
                ?.Single(a => a.GetType() == typeof(RollingFileAppender)) as
                RollingFileAppender;
            var curPath = appender.File;
            var curFile = Path.GetFileName(curPath);
            appender.File = curPath.Replace(curFile, "Test" + curFile);
            appender.ActivateOptions();
        }
    }
}