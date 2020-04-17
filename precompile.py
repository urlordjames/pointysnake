from parse import parse

def psncompile(filename):
    parsed = parse(filename)
    print(parsed)
    f = open("makebin/bin/Debug/precomp.psnbin", "w")
    for line in parsed:
        if line[0] == "call":
            f.write(str(line[2]) + "\n" + line[0] + ", " + line[1] + "\n")
    f.close()

if __name__ == "__main__":
    psncompile("test/consoleout.psn")
