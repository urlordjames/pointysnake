import os
import subprocess
import sys
tests = os.listdir("./tests/")
print("running tests...")

os.system("chmod +x compile.sh")
os.system("mkdir precompiled")

def runcmd(command):
    executable = subprocess.call(command)
    if not executable == 0:
        sys.exit(executable)

for test in tests:
    print(test)
    runcmd(["./compile.sh", "./tests/" + test])
    runcmd(["cp", "precomp.psnbin", "precompiled/" + test + ".psnbin"])

print("done")