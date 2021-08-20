using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace LiveIngestEndToEndTests.Framework
{
    public class TestDataVerifier
    {
        private static readonly string TestDataLockFile = "test-data.lock";

        private static readonly string TestDataRoot =
            Environment.GetEnvironmentVariable("TEST_DATA_DIR");

        private static readonly Regex LinePattern = new(@"^(\S+) (\d+)$");

        public static void VerifyTestData()
        {
            List<string> files = new();

            // Check all files in lock file exist and are the expected size
            foreach (var line in File.ReadLines(TestDataLockFile))
            {
                var match = LinePattern.Match(line);
                if (!match.Success)
                    throw new Exception($"{line} is not correctly formatted");
                var fileName = match.Groups[1].Value;
                var expectedSize = long.Parse(match.Groups[2].Value);
                var path = Path.Join(TestDataRoot, fileName);
                files.Add(path);
                var fileInfo = new FileInfo(path);
                if (!fileInfo.Exists)
                    throw new Exception(
                        $"File {fileName} does not exist in test data directory");

                if (fileInfo.Length != expectedSize)
                    throw new Exception(
                        $"{fileName} was expected to be {expectedSize} bytes, but" +
                        $" was {fileInfo.Length}. Try regenerating the {TestDataLockFile} file");
            }

            // Check all files in the test data root were in the lock file
            var untrackedFiles = WalkFiles(TestDataRoot)
                .Where(f => !files.Contains(f))
                .ToList();
            if (untrackedFiles.Count > 0)
                throw new Exception(
                    "Files exist in the test data root which are not in " +
                    $"{TestDataLockFile}: [{string.Join(", ", untrackedFiles)}]." +
                    " Please regenerate the lock file.");
        }

        private static IEnumerable<string> WalkFiles(string root)
        {
            List<string> files = new();
            Stack<string> children = new();
            children.Push(root);
            while (children.Count > 0)
            {
                var dir = children.Pop();
                foreach (var child in Directory.GetDirectories(dir))
                {
                    children.Push(child);
                }

                files.AddRange(Directory.GetFiles(dir));
            }

            return files;
        }
    }
}
