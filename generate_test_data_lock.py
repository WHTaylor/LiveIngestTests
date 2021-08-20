#!/usr/bin/env python
import os

def file_size(fname):
    with open(fname, 'rb') as f:
        f.seek(0, 2)
        return f.tell()

test_data_root = "test-data"

def rebuild():
    with open("test-data.lock", "w") as f:
        for dir_root, dirs, files in os.walk(test_data_root):
            without_root = os.sep.join(dir_root.split(os.sep)[1:])
            for fname in files:
                fpath = os.path.join(dir_root, fname)
                f.write(f"{os.path.join(without_root, fname)} {file_size(fpath)}\n")

rebuild()
