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
    if line[0][0] == "function":
        if line[1][0] == "string" or line[1][0] == "int":
            return ["call", line[0][1], [parseline(line[1][1])]]
    return -1

if __name__ == "__main__":
    print(parse("test/consoleout.psn"))