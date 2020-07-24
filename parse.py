from lex import lex

constants = ["str", "int", "bool"]
validargs = ["str", "int", "bool", "var", "staticvar"]
rawtypes = [str, int, bool]

def parse(filename):
    lexed = lex(filename)

    ast = []

    for line in lexed:
        ast.append(parseline(line))

    return ast

def parseline(line):
    if type(line) in rawtypes:
        return line
    elif line[0] in constants:
        return line
    elif line[0] == "args":
        if len(line[1]) >= 1:
            buffer = []
            altbuffer = []
            line[1].append(["argseperate"])
            funcscope = 0
            for arg in line[1]:
                if arg[0] == "function":
                    altbuffer.append(arg)
                    funcscope += 1
                elif arg[0] == "functionend":
                    altbuffer.append(arg)
                    funcscope -= 1
                elif funcscope == 0 and arg[0] == "argseperate":
                    if len(altbuffer) > 1:
                        buffer.append(altbuffer)
                    else:
                        buffer.append(altbuffer[0])
                    altbuffer = []
                else:
                    altbuffer.append(arg)
            for arg in buffer:
                if arg[0] in validargs:
                    altbuffer.append(arg)
                else:
                    altbuffer.append(parseline(arg))
            return altbuffer
        return []
    elif line[0][0] in constants:
        return parseline(line[0])
    elif line[0][0] == "functiondefine":
        return ["newfunc", line[1][1]]
    elif line[0][0] == "functionterminate":
        return ["endfunc"]
    elif line[0][0] == "setvar":
        if line[4][0] == "function":
            return [line[0][0], "int", line[2][1], parseline(line[4:])]
        return [line[0][0], line[1][1], line[2][1], parseline(line[4])]
    elif line[0][0] == "setstaticvar":
        if line[4][0] == "function":
            raise Exception("error: staticvar cannot be set as returned value from function")
        return [line[0][0], line[1][1], line[2][1], parseline(line[4])]
    elif line[0][0] == "function":
        if line[1][0] == "functionend":
            return ["call", line[0][1], []]
        else:
            return ["call", line[0][1], parseline(["args", line[1:-1]])]
    elif line[0][0] == "assert":
        return [line[0][0], line[1]]
    elif line[0][0] == "ifdefine":
        return [line[0][0], [parseline(line[1:-2])]]
    raise Exception("token unknown")

if __name__ == "__main__":
    print(parse("tests/printstr.psn"))