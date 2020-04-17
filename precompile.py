from parse import parse

def psncompile(filename):
    return parse(filename)

if __name__ == "__main__":
    print(psncompile("test/consoleout.psn"))
