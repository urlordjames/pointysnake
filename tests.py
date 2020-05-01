import os
import subprocess
import sys
tests = os.listdir("./tests/")
print("running tests...")

os.system("chmod +x compile.sh")

for test in tests:
    print(test)
    run(["./compile.sh", "./tests/", test])
    run(["mono", "executable.exe"])
    run(["rm", "executable.exe"])

def run(command):
    executable = subprocess.call(command)
    if not executable == 0:
        sys.exit(executable)

print("done")