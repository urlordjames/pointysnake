from parse import parse

vartypes = {}

def psncompile(filename):
    parsed = parse(filename)
    print(parsed)
    f = open("precomp.psnbin", "w")
    for line in parsed:
        if line[0] == "call":
            resolvecall(line, f)
        elif line[0] == "setvar":
            vartypes[line[2]] = line[1]
            f.write("setvar, " + line[2] + ", " + line[1] + ", " + str(line[3]) + "\n")
        elif line[0] == "newfunc":
            f.write("newfunc, " + line[1] + "\n")
        elif line[0] == "endfunc":
            f.write("endfunc\n")
        elif line[0] == "cond":
            f.write("ldvar, " + line[1][1] + "\n")
            f.write(line[0] + ", " + line[2][1] + "\n")
    f.close()

def resolvecall(line, f):
    fname = line[1]
    for arg in line[2]:
        if type(arg) == list:
            if arg[0] == "ldvar":
                f.write("ldvar, " + arg[1] + "\n")
            if arg[0] == "call":
                resolvecall(line[2][0], f)
        if type(arg) == str:
            f.write("ldstr, " + arg + "\n")
        if type(arg) == int:
            f.write("ldint, " + str(arg) + "\n")
    if fname == "return":
        if line[2] == []:
            f.write("ldint, 0\n")
        f.write(line[0] + ", " + fname + "\n")
        return
    f.write(line[0] + ", " + fname + "\n")

if __name__ == "__main__":
    psncompile("tests/printstr.psn")
