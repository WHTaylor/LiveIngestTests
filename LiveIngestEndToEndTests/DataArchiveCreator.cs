using System.IO;

namespace LiveIngestEndToEndTests
{
    public static class DataArchiveCreator
    {
        public static string CreateDataArchive()
        {
            var root = Path.Combine(Path.GetTempPath(), "TestArchive");
            Directory.CreateDirectory(root);
            foreach (var instrument in Constants.InstrumentNames)
            {
                Directory.CreateDirectory(Path.Combine(root, $"NDX{instrument}"));
            }
            return root;
        }
    }
}