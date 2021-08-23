#!/usr/bin/env python
""" Generates a lock file to track the files in the test data directory.

The TEST_DATA_DIR environment variable specifies the test data directory.

The output is a file, 'test-data.lock', each line of which represents a file
in the directory in the format:

```
<path relative to test data directory> <file size in bytes>
```

When the end to end test suite runs, it checks that the files in the data
directory match what is contained in 'test-data.lock', and error if not.
"""
import os

def file_size(fpath):
    with open(fpath, 'rb') as f:
        f.seek(0, 2)
        return f.tell()

def rebuild():
    print(f"Rebuilding data lock from {os.environ['TEST_DATA_DIR']}")
    file_count = 0
    with open("test-data.lock", "w") as f:
        for dir_root, dirs, files in os.walk(os.environ["TEST_DATA_DIR"]):
            without_root = os.sep.join(dir_root.split(os.sep)[1:])
            for fname in files:
                fpath = os.path.join(dir_root, fname)
                f.write(f"{os.path.join(without_root, fname)} {file_size(fpath)}\n")
                file_count += 1
    print(f"Found {file_count} files")

if __name__ == "__main__":
    rebuild()
