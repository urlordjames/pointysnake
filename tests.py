import os
import subprocess
import sys
tests = os.listdir("./tests/")
print("running tests...")

os.system("chmod +x compile.sh")

for test in tests:
    print(test)
    os.system("./compile.sh ./tests/" + test)
    executable = subprocess.call(["mono", "executable.exe"])
    if not executable == 0:
        sys.exit(executable)
    os.system("rm executable.exe")

print("done")