#!/bin/sh

python build.py $1
mono makebin.exe
echo "done"