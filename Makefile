.PHONY: test clean
SOURCES := $(shell  find . -type f \( -not -path "*/obj/*" -and -not -path "*/bin/*" -and -not -path "*/* References/*" \) -and \( -name \*.cs -or -name \*.config \))

test: LiveIngestEndToEndTests/bin/Debug/net5.0/LiveIngestEndToEndTests.dll
	dotnet test --no-build

LiveIngestEndToEndTests/bin/Debug/net5.0/LiveIngestEndToEndTests.dll: $(SOURCES)
	nuget restore LiveIngestEndToEndTests.sln
	MSBuild LiveIngestEndToEndTests.sln

clean:
	find . -name obj -or -name bin -exec rm -r {} +
