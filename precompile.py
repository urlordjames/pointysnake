from parse import parse

validtypes = ["str", "int", "bool"]

vartypes = {}

staticvars = {}

def psncompile(filename):
    parsed = parse(filename)
    print(parsed)
    f = open("precomp.psnbin", "w")
    for line in parsed:
        compileline(line, f)
    f.close()

def compileline(line, f):
    if line[0] in validtypes:
        f.write(f"ld{line[0]}, {str(line[1])}\n")
    elif line[0] == "call":
        resolvecall(line, f)
    elif line[0] == "setvar":
        vartypes[line[2]] = line[1]
        compileline(line[3], f)
        f.write(f"setvar, {line[2]}, {line[1]}\n")
    elif line[0] == "setstaticvar":
        staticvars[line[2]] = [line[1], line[3]]
    elif line[0] == "newfunc":
        f.write(f"newfunc, {line[1]}\n")
    elif line[0] == "endfunc":
        f.write("endfunc\n")
    elif line[0] == "cond":
        resolvevar(line[1], f)
        f.write(f"{line[0]}, {line[2][1]}\n")
    elif line[0] == "assert":
        resolvevar(line[1], f)
        f.write(f"{line[0]}\n")

def resolvecall(line, f):
    fname = line[1]
    for arg in line[2]:
        if arg[0] == "var" or arg[0] == "staticvar":
            resolvevar(arg, f)
        elif arg[0] == "call":
            resolvecall(line[2][0], f)
        elif type(arg[0]) == list:
            for value in arg:
                if value[0] == "var" or value[0] == "staticvar":
                    resolvevar(value, f)
                else:
                    compileline(value, f)
    if fname == "return":
        if line[2] == []:
            f.write("ldint, 0\n")
        f.write(f"{line[0]}, {fname}\n")
        return
    f.write(f"{line[0]}, {fname}\n")

def resolvevar(variable, f):
    if variable[0] == "var":
        f.write(f"ldvar, {variable[1]}\n")
    else:
        staticinfo = staticvars[variable[1]]
        f.write(f"ld {staticinfo[0]}, {str(staticinfo[1])}\n")

if __name__ == "__main__":
    psncompile("tests/printstr.psn")
