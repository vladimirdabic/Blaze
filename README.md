# Blaze
**Blaze** is an embeddable dynamic programming language.\
It runs by interpreting bytecode instructions with a stack based interpreter.

For more information, check out the [Documentation](https://vladimirdabic.github.io/Blaze/).

## Usage
```
Usage: blzc.exe [options]
Options:
    -s [file]       Blaze source file to compile
    -m [name]       Use a custom module name
    -d              Compiles as a debug module

Usage: blzi.exe [options]
Options:
    -m [file]       Module file to execute
    -d              Print the contents of the module file
```

## Example
```
extern var console;

func main() {
    console.print("Hello World");
}
```
```
> .\blzc.exe -s hw.blz
  Compiled to 'hw.blzm'
> .\blzi.exe -m hw.blzm
  Hello World
```
This is a simple hello world program. Notice that we have to specify that the console variable is external.\
Blaze doesn't have a concept of a *global environment* like most embeddable/scripting languages do.

Blaze works with **_modules_**.\
Each Blaze source file is compiled to a *module*, which contains functions, classes and variables.\
Variables can be declared as public, private or extern.

The console interface is provided from an external library that contains a module.

During runtime each module can have multiple children and a parent which make up a hierarchy.
