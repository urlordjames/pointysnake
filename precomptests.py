import os
import subprocess
import sys
tests = os.listdir("./tests/")
print("precompiling tests...")

os.system("mkdir precompiled")

def runcmd(command):
    executable = subprocess.call(command)
    if not executable == 0:
        sys.exit(executable)

for test in tests:
    print(test)
    runcmd(["python", "build.py", "./tests/" + test])
    runcmd(["cp", "precomp.psnbin", "precompiled/" + test + ".psnbin"])

print("done")