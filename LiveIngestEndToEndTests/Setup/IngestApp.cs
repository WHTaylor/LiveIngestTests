using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace LiveIngestEndToEndTests.Setup
{
    // ReSharper disable once InconsistentNaming
    public enum Application
    {
        FileWatcher, LiveMonitor, XMLtoICAT,
    }

    public class IngestProcess
    {
        private static readonly string ApplicationsDir =
            Environment.GetEnvironmentVariable("APPLICATIONS_DIR")
            ?? Path.Join("C:", "FBS", "Apps", "ActiveIngest");

        private static readonly List<string> SuccessMarkers = new()
        {
            "waiting for files", // FileWatcher
            "initial setup complete", //LiveMonitor
            "setup was successful", // XMLtoICAT
        };

        private static readonly List<string> FailureMarkers = new()
        {
            "exiting",
            "exception"
        };

        private readonly string _exe;
        private Process _proc;

        public IngestProcess(Application app)
        {
            _exe = Path.Join(ApplicationsDir, app.ToString(), $"{app}.exe");
            if (!File.Exists(_exe))
            {
                throw new FileNotFoundException($"{_exe} does not exist");
            }
        }

        public void Start(params string[] args)
        {
            var fullArgs = args.Concat(new[] {"--test-mode"});
            StartProc(fullArgs);

            TestContext.Progress.WriteLine("Waiting for app to be setup...");
            var success = WaitForReady(1);
            if (!success)
                throw new ApplicationException($"{_exe} failed to start");
        }

        private void StartProc(IEnumerable<string> args)
        {
            TestContext.Progress.WriteLine($"Starting {_exe}");
            _proc = new Process
            {
                StartInfo = new ProcessStartInfo(_exe)
                {
                    Arguments = string.Join(" ", args),
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                }
            };
            _proc.Start();
        }

        /// <summary>
        /// Wait up to timeoutSeconds for the process to be ready for ingestion.
        ///
        /// Two limitations of this implementation are because ReadLine() is
        /// blocking - a) if the process hangs at startup, this will also hang,
        /// and b) timeouts happen after the first output line after timeoutSeconds,
        /// not at timeoutSeconds exactly.
        /// </summary>
        /// <param name="timeoutSeconds">Maximum seconds to wait</param>
        /// <returns>
        /// False if the process ends, timeoutSeconds pass, or an error appears in
        /// the log, otherwise true.
        /// </returns>
        private bool WaitForReady(int timeoutSeconds)
        {
            var timer = new Stopwatch();
            timer.Start();
            var error = false;
            var success = false;
            while (!error && !success)
            {
                var readLine = _proc.StandardOutput.ReadLine();
                TestContext.Progress.WriteLine(readLine);

                if (readLine == null)
                {
                    TestContext.Progress.WriteLine(
                        "Startup aborted unexpectedly");
                    error = true;
                }
                else if (SuccessMarkers.Any(m => readLine.ToLower().Contains(m)))
                {
                    TestContext.Progress.WriteLine($"Success on: {readLine}");
                    success = true;
                }
                else if (FailureMarkers.Any(m => readLine.ToLower().Contains(m)))
                {
                    TestContext.Progress.WriteLine($"Error on: {readLine}");
                    error = true;
                }
                else if (timer.Elapsed.Seconds > timeoutSeconds)
                {
                    TestContext.Progress.WriteLine("Startup timed out");
                    error = true;
                }
            }

            return success;
        }

        public void Stop()
        {
            if (!_proc.HasExited) _proc.Kill();
        }
    }
}
