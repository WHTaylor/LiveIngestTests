using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.WebSockets;
using NUnit.Framework;

namespace LiveIngestEndToEndTests
{
    // ReSharper disable once InconsistentNaming
    public enum Application
    {
        FileWatcher,
        LiveMonitor,
        XMLtoICAT,
    }

    public class IngestProcess
    {
        private static readonly string ApplicationsDir =
            Environment.GetEnvironmentVariable("APPLICATIONS_DIR")
            ?? Path.Join("C:", "FBS", "Apps", "ActiveIngest");

        private static readonly List<string> SuccessMarkers = new()
        {
            "waiting for files. watching directory", // FileWatcher
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

            TestContext.Progress.WriteLine("Waiting to be setup...");

            var error = false;
            var success = false;
            while (!error && !success)
            {
                string readLine;
                while ((readLine = _proc.StandardOutput.ReadLine()) == null)
                {
                    //wait a bit
                }

                if (FailureMarkers.Any(m => readLine.ToLower().Contains(m)))
                {
                    TestContext.Progress.WriteLine($"Error on: {readLine}");
                    error = true;
                }
                else if (SuccessMarkers.Any(m => readLine.ToLower().Contains(m)))
                {
                    TestContext.Progress.WriteLine($"Success on: {readLine}");
                    success = true;
                }
            }

            if (error) throw new ApplicationException($"{_exe} failed to start");
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

        public void Stop()
        {
            if (!_proc.HasExited) _proc.Kill();
        }
    }
}