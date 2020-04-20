import os
tests = os.listdir("./tests/")
print("running tests...")

os.system("chmod +x compile.sh")

for test in tests:
    print(test)
    os.system("./compile.sh ./tests/" + test)
    os.system("mono executable.exe")
    os.system("rm executable.exe")

print("done")