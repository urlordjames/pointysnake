# pointysnake
A somewhat essoteric programming language built using C# (pointy) and Python (snake).  Pointysnake is a statically typed (not yet, but should be soon), minimalistic, CIL compiled, functional programming language with a focus on having the benefits of both Python and C# while *attempting* to negate the problems of both.

# pointysnake is not ready for use in serious projects, please submit an issue if you encounter a bug

# hello world program
```
print("hello world!")
```

That's it, one line, no defining entrypoints, classes, or namespaces.

# how to compile

- make sure you have dotnet or mono runtime installed (I'm fairly certain either will work)
- make sure python3.7 is installed **there is a bug in 3.5 that causes precompilation to randomly fail half the time!**
- clone the github repo
- create a file (although it's not enforced, a .psn extension is reccomended) and write your code
- precompile your code by running `python build.py (filename)`.
- build the compiler from source, or go to the Github actions page and download the artifact called `compiler`
- place the generated file called `precomp.psnbin` from the python script in the same folder as `makebin.exe` and run it
- executable called `executable.exe` will be created
- congratulations, you've compiled pointysnake!