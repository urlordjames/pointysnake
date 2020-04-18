#!/usr/bin/python

import sys
from precompile import psncompile

try:
    psncompile(sys.argv[0])
except Exception as e:
    print(e)
    exit(1)