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
    "^cond$": ["conddefine"],
    "^assert$": ["assert"],
    "^\|$": ["condseperate"],
    "^{$": ["functionstart"],
    "^}$": ["functionterminate"],
    "^,$": ["argseperate"],
    "^[a-z]+\\($": ["function"],
    "^\\d+$": ["int"],
    "^\".*\"$": ["str"],
    "^\\)$": ["functionend"],
    "^var [a-zA-Z]+ = $": ["setvar"],
    "^staticvar [a-zA-Z]+ = $": ["setstaticvar"],
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
            if not match == None:
                matches.append([match, tokens[token]])
                buffer = ""
                continue
    tokenized = []
    for match in matches:
        if match[1][0] == "ignore":
            continue
        if match[1][0] == "function":
            tokenized.append([match[1][0], match[0].group()[:-1]])
            continue
        if match[1][0] == "str":
            tokenized.append([match[1][0], match[0].group()[1:-1]])
            continue
        if match[1][0] == "int":
            #this is ugly and disgusting and needs to be fixed later maybe
            #TODO: fix this mess
            previoustoken = tokenized[len(tokenized) - 1]
            if previoustoken[0] == "int":
                previoustoken[1] = int(str(previoustoken[1]) + str(match[0].group()))
            else:
                tokenized.append([match[1][0], int(match[0].group())])
            continue
        if match[1][0] == "bool":
            tokenized.append([match[1][0], match[0].group()])
            continue
        if match[1][0] == "var":
            tokenized.append([match[1][0], match[1][1]])
            continue
        if match[1][0] == "setvar":
            varname = match[0].group()[4:-3]
            tokenized.append([match[1][0], varname])
            tokens.update({f"^{varname}$": ["var", varname]})
            continue
        if match[1][0] == "setstaticvar":
            varname = match[0].group()[10:-3]
            tokenized.append([match[1][0], varname])
            tokens.update({f"^{varname}$": ["staticvar", varname]})
            continue
        tokenized.append(match[1])
    return tokenized

if __name__ == "__main__":
    print(lex("tests/printstr.psn"))