#!/bin/bash

echo "running tests..."

for f in ./precompiled/*.psnbin; do
    echo $f
    cp $f precomp.psnbin
    mono makebin.exe
    if [ $? -eq 0 ]
    then
        echo "test passed"
        rm precomp.psnbin
    else
        echo "test failed"
        cat precomp.psnbin
        exit 1
    fi
done

echo "done"