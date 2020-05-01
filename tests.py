import os
import subprocess
import sys
tests = os.listdir("./tests/")
print("running tests...")

os.system("chmod +x compile.sh")

def runcmd(command):
    executable = subprocess.call(command)
    if not executable == 0:
        sys.exit(executable)

for test in tests:
    print(test)
    runcmd(["./compile.sh", "./tests/", test])
    runcmd(["mono", "executable.exe"])
    runcmd(["rm", "executable.exe"])

print("done")