#!/bin/sh

cd makebin
nuget restore
msbuild makebin.csproj -p:Configuration=Release
cp bin/Release/makebin.exe .. && cp bin/Release/dnlib.dll ..
cd ..