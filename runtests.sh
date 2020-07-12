#!/bin/bash

echo "running tests..."

for f in ./precompiled/*.psnbin; do
    echo $f
    mono makebin.exe $f
    if [ $? -eq 0 ]
    then
        echo "test passed"
    else
        echo "test failed"
    exit 1
    fi
done

echo "done"