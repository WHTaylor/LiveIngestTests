using System;
using System.IO;
using System.Linq;

namespace LiveIngestEndToEndTests.Framework
{
    public class DataFileCopier
    {
        private readonly string _testDataRoot;
        private readonly string _archiveRoot;
        private readonly string _instrument;
        private string InstDir => Path.Join(_testDataRoot, _instrument);

        /// <summary>
        /// Create a DataFileCopier. Must be called after TempDataArchive::Create
        /// </summary>
        /// <param name="instrument"> The instrument the data files to be copied
        /// are for. Controls directory files are copied to </param>
        public DataFileCopier(string instrument)
        {
            if (!Constants.InstrumentNames.Contains(instrument.ToUpper()))
            {
                throw new InvalidDataException(
                    $"{instrument} is not a valid instrument. Must be one of:" +
                    $"[{string.Join(", ", Constants.InstrumentNames)}]");
            }

            _testDataRoot = Environment.GetEnvironmentVariable("TEST_DATA_DIR");
            _archiveRoot = TempDataArchive.RootDir;
            _instrument = instrument.ToUpper();
        }

        public void CopyFile(string fileName)
        {
            var filePath = Path.Combine(InstDir, fileName);
            File.Copy(filePath, DestPath(fileName));
        }

        public void CopyRun(string runNumber)
        {
            var runFiles = Directory.EnumerateFiles(InstDir)
                .Where(f => f.Contains(runNumber));
            foreach (var path in runFiles)
            {
                File.Copy(path, DestPath(Path.GetFileName(path)));
            }
        }

        private string DestPath(string fileName)
        {
            return Path.Combine(_archiveRoot, $"NDX{_instrument}", fileName);
        }
    }
}
