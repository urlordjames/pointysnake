def lex(filename):
    f = open(filename, "r")
    src = f.read()
    f.close()

    lines = src.split("\n")
    newlines = []

    for line in lines:
        if line[0] == "#":
            continue
        newlines.append(line)
    
    tokens = []

    for line in newlines:
        tokens.append(tokenizeln(line))

    return tokens

import re

tokens = {
    "[a-z]+\\(": ["function"],
    "\".*\"": ["constant", "string"],
    "\\)": ["functionend"]
}

#return 

def tokenizeln(line):
    matches = []
    for token in tokens.keys():
        matches.append([re.search(token, line), tokens[token]])
    matches = sorted(matches, key=lambda match: match[0].span()[0])
    tokenized = []
    for match in matches:
        if match[1][0] == "function":
            tokenized.append(["function", match[0].group()[:-1]])
            continue
        tokenized.append(match[1])
    return tokenized

if __name__ == "__main__":
    lex("test/consoleout.psn")