# LiveIngestEndToEndTests

A test suite which runs test experimental data through the full live ingest. The original design plan can be found [here](https://github.com/ral-facilities/isis-tasks/issues/22#issuecomment-885040628).

## Usage

Must be run on a machine with the ingest applications available, and which can connect to the dev ICAT and AMQ instances.

Requires the following environment variables:
 - `TEST_DATA_DIR` - the directory containing the test data.
 - `APPLICATIONS_DIR` - the directory containing the applications to test. Must have FileWatcher, LiveMonitor, and XMLtoICAT subdirectores, each of which contains the corresponding exe and everything needed for it to run.
 - `AMQ_BROKER_URL` - the (tcp) URL of the ActiveMQ broker the tests applications run against.

To build and run[<sup>1</sup>](#footnote1):
```
nuget restore LiveIngestEndToEndTests.sln
MSBuild.exe LiveIngestEndToEndTests.sln
dotnet test --no-build
```

There's a hacky makefile also included which simplifies this to `make`, if you have it installed.

## Structure

 - `Setup.cs` creates and tears down the test environmnent at the beginning of execution.
 - `Framework` contains components for interacting with the external systems (file system, ICAT, AMQ), which are used in both setup and the tests.
 - `Tests` contains the actual tests.

---

<a name="footnote1">1: </a>This couldn't be a straightforward `dotnet test` because ICAT4IngestLibrary, a .NET framework project using the old csproj style, is used as a ProjectReference to include it in the build. `dotnet build` can't build it, but nunit projects for .NET core must be run with `dotnet test`, which runs `dotnet build` implicitly - so we have to build then test as separate steps. Tested with nuget 5.9.1.11, MSBuild 16.10.2, and dotnet 5.0.301.
</aside>
