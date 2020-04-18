def lex(filename):
    f = open(filename, "r")
    src = f.read()
    f.close()

    lines = src.split("\n")
    newlines = []

    for line in lines:
        if len(line) < 1 or line[0] == "#":
            continue
        newlines.append(line)
    
    tokens = []

    for line in newlines:
        tokens.append(tokenizeln(line))

    return tokens

import re

tokens = {
    "[a-z]+\\(": ["function"],
    "[0-9]+": ["int"],
    "\".*\"": ["str"],
    "\\)": ["functionend"],
    "var [a-z]+ = ": ["setvar"],
    "\\$[a-z]+": ["var"]
}

def tokenizeln(line):
    matches = []
    for token in tokens.keys():
        result = re.search(token, line)
        if result == None:
            continue
        matches.append([result, tokens[token]])
    matches = sorted(matches, key=lambda match: match[0].span()[0])
    tokenized = []
    for match in matches:
        if match[1][0] == "function":
            tokenized.append([match[1][0], match[0].group()[:-1]])
            continue
        if match[1][0] == "str":
            tokenized.append([match[1][0], match[0].group()[1:-1]])
            continue
        if match[1][0] == "int":
            tokenized.append([match[1][0], int(match[0].group())])
            continue
        if match[1][0] == "var":
            tokenized.append([match[1][0], match[0].group()[1:]])
            continue
        if match[1][0] == "setvar":
            tokenized.append([match[1][0], match[0].group()[4:-3]])
            continue
        tokenized.append(match[1])
    return tokenized

if __name__ == "__main__":
    print(lex("test/tests.psn"))