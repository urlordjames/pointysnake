#!/bin/bash

echo "running tests..."

for f in ./precompiled/*.psnbin; do
    echo $f
    mono makebin.exe ./precompiled/$f
done

echo "done"