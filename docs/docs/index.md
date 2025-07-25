# Blaze
Blaze is an embeddable dynamic programming language.

Download the latest build from [GitHub](https://github.com/vladimirdabic/Blaze) and extract the zip archive. You may add Blaze to your PATH environment variable if you wish.

To confirm that Blaze is installed properly, open a command prompt and try running the compiler `blzc` or the interpreter `blzi`:
```none
> blzc
Usage: blzc.exe [options]
Options:
    -s [file]       Blaze source file to compile
    -m [name]       Use a custom module name
    -d              Compiles as a debug module
    -c              Print the contents of a module file (source file must be blzm)

> blzi
Usage: blzi.exe [options]
Options:
    -m [file]       Module file to execute
    -d              Print the contents of the module file
```