# LiveIngestEndToEndTests

Gosh what a mouthful of a name

## Usage

Requires the following environment variables to be set:
 - `TEST_DATA_DIR` - the directory containing the test data.
 - `APPLICATIONS_DIR` - the directory containing the applications to test. Must have FileWatcher, LiveMonitor, and XMLtoICAT subdirectores, each of which contains the corresponding exe and everything needed for it to run.
 - `BROKER_URL` - the (tcp) URL of the ActiveMQ broker the tests applications run against.
