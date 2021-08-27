using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using NUnit.Framework;

namespace LiveIngestEndToEndTests.Framework
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
        private readonly List<string> _output = new();

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
            var success = WaitForReady(5);
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

            // Asynchronously read all stdout to _output
            // We use the beginning of output to check the process has started
            // correctly. All output must be consumed, otherwise the stdout
            // buffer fills and log4net causes the ingest process to hang.
            _proc.OutputDataReceived += (p, o) => _output.Add(o.Data);
            _proc.Start();
            _proc.BeginOutputReadLine();
        }

        /// <summary>
        /// Wait up to timeoutSeconds for the process to be ready for ingestion.
        /// </summary>
        /// <param name="timeoutSeconds">Maximum seconds to wait</param>
        /// <returns>
        /// False if the process ends, timeoutSeconds pass, or an error appears in
        /// the log, otherwise true.
        /// </returns>
        private bool WaitForReady(int timeoutSeconds)
        {
            var error = false;
            var success = false;
            var lineToRead = 0;
            var timer = new Stopwatch();
            timer.Start();
            while (!error && !success)
            {
                while (_output.Count <= lineToRead)
                {
                    if (timer.Elapsed.Seconds > timeoutSeconds)
                    {
                        TestContext.Error.WriteLine("Startup timed out");
                        return false;
                    }

                    Thread.Sleep(20);
                }

                var readLine = _output[lineToRead];
                lineToRead++;

                if (readLine == null)
                {
                    TestContext.Error.WriteLine("Startup aborted unexpectedly");
                    error = true;
                }
                else if (SuccessMarkers.Any(m => readLine.ToLower().Contains(m)))
                {
                    TestContext.Progress.WriteLine($"Success on: {readLine}");
                    success = true;
                }
                else if (FailureMarkers.Any(m => readLine.ToLower().Contains(m)))
                {
                    TestContext.Error.WriteLine($"Error on: {readLine}");
                    error = true;
                }
            }

            return success;
        }

        public void Stop()
        {
            if (_proc.HasExited) return;
            TestContext.Progress.WriteLine($"Stopping {_exe}");
            _proc.Kill();
        }
    }
}
