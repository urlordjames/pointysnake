from lex import lex

def parse(filename):
    lexed = lex(filename)

    ast = []

    for line in lexed:
        ast.append(parseline(line))

    return ast

def parseline(line):
    if type(line) == str or type(line) == int:
        return line
    if line[0][0] == "functiondefine":
        return ["newfunc", line[1][1]]
    if line[0][0] == "functionterminate":
        return ["endfunc"]
    if line[0][0] == "setvar":
        return ["setvar", line[1][0], line[0][1], parseline(line[1][1])]
    if line[0][0] == "function":
        if line[1][0] == "str" or line[1][0] == "int":
            return ["call", line[0][1], [parseline(line[1:-1])]]
        if line[1][0] == "function":
            return ["call", line[0][1], [parseline(line[1:-1])]]
        if line[1][0] == "var":
            return ["call", line[0][1], [["ldvar", line[1][1]]]]
        return ["call", line[0][1], []]
    if line[0][0] == "conddefine":
        return ["cond", line[1], line[3]]
    if line[1][0] == "argseperate":
        return [["ldvar", line[0][1]], ["ldvar", line[2][1]]]
    return -1

if __name__ == "__main__":
    print(parse("tests/printstr.psn"))