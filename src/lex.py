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
    "^function$": ["functiondefine"],
    "^assert$": ["assert"],
    "^{$": ["functionstart"],
    "^}$": ["functionterminate"],
    "^,$": ["argseperate"],
    "^int$": ["type", "int"],
    "^str$": ["type", "str"],
    "^bool$": ["type", "bool"],
    "^if$": ["ifdefine"],
    "^while$": ["whiledefine"],
    "^[a-z]+\\($": ["function"],
    "^\\d+$": ["int"],
    "^\".*\"$": ["str"],
    "^\\($": ["argopen"],
    "^\\)$": ["argend"],
    "^var $": ["setvar"],
    "^staticvar $": ["setstaticvar"],
    "^[a-zA-Z]+ $": ["assignvar"],
    "^=$": ["assignop"],
    "^true|false$": ["bool"],
    "^[ |\t]$": ["ignore"]
}

def tokenizeln(line):
    buffer = ""
    matches = []
    for char in line:
        buffer += char
        for token in tokens.keys():
            match = re.search(token, buffer)
            if not match is None:
                matches.append([match, tokens[token]])
                buffer = ""
                break
    tokenized = []
    for match in matches:
        if match[1][0] == "ignore":
            continue
        elif match[1][0] == "function":
            tokenized.append([match[1][0], match[0].group()[:-1]])
            continue
        elif match[1][0] == "str":
            tokenized.append([match[1][0], match[0].group()[1:-1]])
            continue
        elif match[1][0] == "int":
            previoustoken = tokenized[len(tokenized) - 1]
            if previoustoken[0] == "int":
                previoustoken[1] = int(str(previoustoken[1]) + match[0].group())
            else:
                tokenized.append([match[1][0], int(match[0].group())])
            continue
        elif match[1][0] == "bool":
            tokenized.append([match[1][0], match[0].group()])
            continue
        elif match[1][0] == "assignvar":
            try:
                oldthing = tokenized[len(tokenized) - 2][0]
            except:
                continue
            if not (oldthing == "setvar" or oldthing == "setstaticvar"):
                continue
            varname = match[0].group()[:-1]
            tokenized.append([match[1][0], varname])
            tokens.update({f"^{varname}$": [tokenized[len(tokenized) - 3][0][3:], varname]})
            continue
        tokenized.append(match[1])
    return tokenized

if __name__ == "__main__":
    print(lex("../tests/printstr.psn"))
