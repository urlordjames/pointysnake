from parse import parse

vartypes = {}

staticvars = {}

def psncompile(filename):
    parsed = parse(filename)
    print(parsed)
    f = open("precomp.psnbin", "w")
    for line in parsed:
        if line[0] == "call":
            resolvecall(line, f)
        elif line[0] == "setvar":
            vartypes[line[2]] = line[1]
            if type(line[3]) == list:
                resolvecall(line[3], f)
                f.write("setvar, " + line[2] + ", " + line[1] + ", pop\n")
            else:
                f.write("setvar, " + line[2] + ", " + line[1] + ", " + str(line[3]) + "\n")
        elif line[0] == "setstaticvar":
            staticvars[line[2]] = [line[1], line[3]]
        elif line[0] == "newfunc":
            f.write("newfunc, " + line[1] + "\n")
        elif line[0] == "endfunc":
            f.write("endfunc\n")
        elif line[0] == "cond":
            resolvevar(line[1], f)
            f.write(line[0] + ", " + line[2][1] + "\n")
        elif line[0] == "assert":
            resolvevar(line[1], f)
            f.write(line[0] + "\n")
    f.close()

def resolvecall(line, f):
    fname = line[1]
    for arg in line[2]:
        if type(arg) == list:
            if arg[0] == "ldvar" or arg[0] == "ldstaticvar":
                resolvevar(arg, f)
            if arg[0] == "call":
                resolvecall(line[2][0], f)
            if type(arg[0]) == list:
                for value in arg:
                    if value[0] == "var" or value[0] == "staticvar":
                        resolvevar(value, f)
                    else:
                        f.write("ld" + value[0] + ", " + str(value[1]) + "\n")
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

def resolvevar(variable, f):
    if variable[0] == "var":
        f.write("ldvar, " + variable[1] + "\n")
    else:
        staticinfo = staticvars[variable[1]]
        f.write("ld" + staticinfo[0] + ", " + str(staticinfo[1]) + "\n")

if __name__ == "__main__":
    psncompile("tests/printstr.psn")
