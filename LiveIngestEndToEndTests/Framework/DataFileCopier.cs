using System;
using System.IO;
using System.Linq;

namespace LiveIngestEndToEndTests.Framework
{
    public class DataFileCopier
    {
        private readonly string _testDataRoot;
        private readonly string _archiveRoot;
        private string _instrument;

        public DataFileCopier()
        {
            _testDataRoot = Environment.GetEnvironmentVariable("TEST_DATA_DIR");
            _archiveRoot = TempDataArchive.RootDir;
        }

        public DataFileCopier ForInstrument(string instrumentName)
        {
            _instrument = instrumentName;
            return this;
        }

        public void CopyFile(string fileName)
        {
            var filePath = Path.Combine(_testDataRoot, fileName);
            File.Copy(filePath, DestPath(fileName));
        }

        public void CopyRun(string runNumber)
        {
            var runFiles = Directory.EnumerateFiles(_testDataRoot)
                .Where(f => f.Contains(runNumber));
            foreach (var path in runFiles)
            {
                File.Copy(path, DestPath(Path.GetFileName(path)));
            }
        }

        private string DestPath(string fileName)
        {
            return Path.Combine(
                _archiveRoot,
                $"NDX{_instrument.ToUpper()}",
                fileName);
        }
    }
}
