from parse import parse

validtypes = ["str", "int", "bool"]

vartypes = {}

staticvars = {}

branchid = 0
branchstack = []

def psncompile(filename):
    parsed = parse(filename)
    print(parsed)
    f = open("precomp.psnbin", "w")
    for line in parsed:
        compileline(line, f)
        if line[0] == "call":
            f.write("pop?\n")
    f.close()

def compileline(line, f):
    global branchid
    if line[0] in validtypes:
        f.write(f"ld{line[0]}, {str(line[1])}\n")
    elif line[0] == "var" or line[0] == "staticvar":
        resolvevar(line, f)
    elif line[0] == "call":
        resolvecall(line, f)
    elif line[0] == "setvar":
        vartypes[line[2]] = line[1]
        compileline(line[3], f)
        f.write(f"setvar, {line[2]}, {line[1]}\n")
    elif line[0] == "setstaticvar":
        staticvars[line[2]] = line[3]
    elif line[0] == "newfunc":
        branchstack.append(["function"])
        f.write(f"newfunc, {line[1]}\n")
    elif line[0] == "endfunc":
        context = branchstack.pop()
        if context[0] == "function":
            f.write("endfunc\n")
        elif context[0] == "if":
            f.write(f"brend, {str(context)}\n")
        elif context[0] == "while":
            f.write(f"brend, {str(context[3])}\n")
            compileline(context[2], f)
            f.write(f"brtrue, {str(branchid)}\n")
            branchid += 1
        elif context[0] == "jmp":
            f.write(f"brend, {str(context)}\n")
        else:
            raise Exception("unknown branch")
    elif line[0] == "assert":
        compileline(line[1], f)
        f.write(f"brtrue, {str(branchid)}\n")
        f.write("ldint, 1\n")
        f.write("call, return\n")
        f.write(f"brend, {str(branchid)}\n")
        branchid += 1
    elif line[0] == "ifdefine":
        compileline(line[1][0], f)
        f.write(f"brfalse, {str(branchid)}\n")
        branchstack.append(["if", branchid])
        branchid += 1
    elif line[0] == "whiledefine":
        old = branchid
        branchid += 1
        f.write(f"br, {str(old)}\n")
        f.write(f"brend, {str(branchid)}\n")
        branchstack.append(["while", branchid, line[1][0], old])
    else:
        raise Exception("unknown grammar")

def resolvecall(line, f):
    fname = line[1]
    for arg in line[2]:
        compileline(arg, f)
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
        f.write(f"ld{staticinfo[0]}, {str(staticinfo[1])}\n")

if __name__ == "__main__":
    psncompile("tests/printstr.psn")
