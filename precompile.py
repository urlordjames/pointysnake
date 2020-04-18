from parse import parse

def psncompile(filename):
    parsed = parse(filename)
    print(parsed)
    f = open("precomp.psnbin", "w")
    for line in parsed:
        if line[0] == "call":
            fname = line[1]
            for arg in line[2]:
                if type(arg) == str:
                    f.write("ldstr, " + arg + "\n")
                if type(arg) == int:
                    f.write("ldint, " + str(arg) + "\n")
            fname += str(type(arg).__name__)
            f.write(line[0] + ", " + fname + "\n")
    f.close()

if __name__ == "__main__":
    psncompile("test/consoleout.psn")
