using System.IO;

namespace LiveIngestEndToEndTests
{
    public static class TempDataArchive
    {
        public static readonly string RootDir =
            Path.Combine(Path.GetTempPath(), "TestArchive");

        private static bool _created;

        public static void Create()
        {
            if (_created) return;

            Directory.CreateDirectory(RootDir);
            foreach (var instrument in Constants.InstrumentNames)
            {
                Directory.CreateDirectory(
                    Path.Combine(RootDir, $"NDX{instrument}"));
            }

            _created = true;
        }
    }
}
