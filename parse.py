from lex import lex

validargs = ["str", "int", "var", "staticvar"]

def parse(filename):
    lexed = lex(filename)

    ast = []

    for line in lexed:
        ast.append(parseline(line))

    return ast

def parseline(line):
    if type(line) == str or type(line) == int:
        return line
    if line[0][0] in validargs:
        if len(line) > 1:
            buffer = []
            for arg in line:
                if arg[0] == "argseperate":
                    continue
                buffer.append([arg[0], arg[1]])
            return buffer
        return parseline(line[0][1])
    if line[0][0] == "functiondefine":
        return ["newfunc", line[1][1]]
    if line[0][0] == "functionterminate":
        return ["endfunc"]
    if line[0][0] == "setvar":
        if line[1][0] == "function":
            return [line[0][0], "int", line[0][1], parseline(line[1:])]
        return [line[0][0], line[1][0], line[0][1], parseline(line[1][1])]
    if line[0][0] == "setstaticvar":
        if line[1][0] == "function":
            print("error: staticvar cannot be set as returned value from function")
            return -1
        return [line[0][0], line[1][0], line[0][1], parseline(line[1][1])]
    if line[0][0] == "function":
        if line[1][0] == "functionend":
            return ["call", line[0][1], []]
        else:
            return ["call", line[0][1], [parseline(line[1:-1])]]
    if line[0][0] == "conddefine":
        return ["cond", line[1], line[3]]
    if line[0][0] == "assert":
        return [line[0][0], line[1]]
    return -1

if __name__ == "__main__":
    print(parse("tests/printstr.psn"))