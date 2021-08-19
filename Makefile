.PHONY: test

test: LiveIngestEndToEndTests/bin/Debug/net5.0/LiveIngestEndToEndTests.dll
	dotnet test --no-build

LiveIngestEndToEndTests/bin/Debug/net5.0/LiveIngestEndToEndTests.dll:
	nuget restore LiveIngestEndToEndTests.sln
	MSBuild LiveIngestEndToEndTests.sln
