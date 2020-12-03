#!/bin/sh

python3 src/build.py $1
if [ $? -eq 0 ]
then
    mono makebin.exe
else
    echo "precompile failed"
    exit 1
fi
