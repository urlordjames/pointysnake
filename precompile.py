from parse import parse

vartypes = {}

def psncompile(filename):
    parsed = parse(filename)
    print(parsed)
    f = open("precomp.psnbin", "w")
    for line in parsed:
        if line[0] == "call":
            fname = line[1]
            for arg in line[2]:
                if type(arg) == list:
                    if arg[0] == "ldvar":
                        f.write("ldvar, " + arg[1] + "\n")
                if type(arg) == str:
                    f.write("ldstr, " + arg + "\n")
                if type(arg) == int:
                    f.write("ldint, " + str(arg) + "\n")
            argtype = str(type(arg).__name__)
            if fname == "print":
                f.write(line[0] + ", " + fname + "\n")
                continue
            if argtype == "list":
                fname += vartypes[arg[1]]
            else:
                fname += argtype
            f.write(line[0] + ", " + fname + "\n")
        elif line[0] == "setvar":
            vartypes[line[2]] = line[1]
            f.write("setvar, " + line[2] + ", " + line[1] + ", " + str(line[3]) + "\n")
    f.close()

if __name__ == "__main__":
    psncompile("test/tests.psn")
