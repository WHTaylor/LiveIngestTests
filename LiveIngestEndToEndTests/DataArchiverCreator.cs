using System.IO;

namespace LiveIngestEndToEndTests
{
    public class DataArchiverCreator
    {
        public string CreateDataArchive()
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