#!/bin/sh

apt-get install zip -y
mkdir /tmp/tozip
cp makebin.exe /tmp/tozip
cp build.py /tmp/tozip
cp precompile.py /tmp/tozip
cp parse.py /tmp/tozip
cp lex.py /tmp/tozip
cp LICENSE /tmp/tozip
zip /tmp/artifacts.zip /tmp/tozip/*