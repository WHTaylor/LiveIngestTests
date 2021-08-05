using System.IO;
using NUnit.Framework;

namespace LiveIngestEndToEndTests.Framework
{
    public static class TempDataArchive
    {
        public static readonly string RootDir;
        private static bool _created;

        static TempDataArchive()
        {
            var rootDir = Path.Combine(
                Path.GetTempPath(),
                "TestArchive");
            var suffix = 1;
            while (Directory.Exists(rootDir))
            {
                rootDir = Path.Combine(
                    Path.GetTempPath(),
                    "TestArchive",
                    suffix.ToString());
                suffix++;
            }

            RootDir = rootDir;
        }

        public static void Create()
        {
            TestContext.Progress.WriteLine(_created);
            if (_created) return;

            Directory.CreateDirectory(RootDir);
            foreach (var instrument in Constants.InstrumentNames)
            {
                Directory.CreateDirectory(
                    Path.Combine(RootDir, $"NDX{instrument}"));
            }

            _created = true;
            TestContext.Progress.WriteLine($"Created data archive at {RootDir}");
        }
    }
}
