#!/bin/bash

echo "running tests..."

for f in ./precompiled/*.psnbin; do
    echo $f
    cp $f precomp.psnbin
    mono makebin.exe
    if [ $? -eq 0 ]
    then
        echo "compiled successfully"
        rm precomp.psnbin
    else
        echo "compilation failure"
        cat precomp.psnbin
        exit 1
    fi
    mono executable.exe
    if [ $? -eq 0 ]
    then
        echo "test passed"
    else
        echo "runtime error"
        exit 1
    fi
done

echo "done"