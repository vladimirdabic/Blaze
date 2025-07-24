# Blaze
**Blaze** is an embeddable dynamic programming language.\
It runs by interpreting bytecode instructions with a stack based interpreter.

_**Why do we need another language?**_\
Well, we don't. It's a project for college.\
Nobody will actually use it for anything serious.\
It's not designed to be super fast or efficient.

Documentation (in Serbian): [PDF](https://drive.google.com/file/d/1I4bpM7I9xLVZhMllnoYMQ4An64-sZTPR/view?usp=sharing)

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


## Features
Blaze currently supports:
- Basic values (Numbers, Strings, Booleans, null)
- Functions
- Classes
- Lists
- Dictionaries
- Events
- All of the expected control flow (if, while, for)

## Events
As Blaze is an embeddable language, it has to have some kind of event system.\
The host application might expose an event to which you can attach a callback.
```
// here the application exposes an object that holds multiple events
extern var UserEvent;
extern var console;

event UserEvent.Connect(username) {
    console.print(username, " connected");
}

event UserEvent.Disconnect(username) {
    console.print(username, " disconnected");
}
```
You can attach multiple callbacks to an event, obviously.

If you want something to happen only if an event parameter is a specific value, you can pass a list of expected values.\
If the parameter is not one of these, the callback won't execute.

```
event UserEvent.Connect(["vladimirdabic", "doofusjack"]) {
    console.print("Specific user connected");
}
```

This is just syntactic sugar for
```
event UserEvent.Connect(username) {
    if(!(["vladimirdabic", "doofusjack"].contains(username))) return;

    console.print("Specific user connected");
}
```

## Libraries
Blaze libraries are made via C# class libraries. A library consists of a collection of modules. To create a library, add a reference to `blzcore.dll`, then use the BlazeModule attribute and implement the IBlazeModuleFactory interface. Here's an example library with one module:
```cs
using VD.Blaze.Interpreter;
using VD.Blaze.Interpreter.Environment;
using VD.Blaze.Interpreter.Types;
using VD.Blaze.Lib;

namespace ExampleLib
{
    [BlazeModule("ExampleModule")]
    public class Example : IBlazeModuleFactory
    {
        public ModuleEnv CreateModule()
        {
            var env = new ModuleEnv();
            env.DefineFunction("testfunc", TestFunc);

            return env;
        }

        private IValue TestFunc(VM vm, List<IValue> args)
        {
            Console.WriteLine("This is my test function");
            return VM.NullInstance;
        }
    }
}
```
Using the function is as simple as:
```
import testfunc;  // import is an alias for 'extern var'

func main() {
    testfunc();
}
```
You may group functions and variables into a single object. This is done by using the Library object. Here's an example:
```cs
public ModuleEnv CreateModule()
{
    ModuleEnv env = new ModuleEnv();
    Library lib = new Library("console");
    env.DefineVariable("console", VariableType.PUBLIC, lib);

    lib.DefineFunction("print", (VM vm, List<IValue> args) =>
    {
        foreach (var arg in args)
            Console.Write(arg.AsString());

        Console.Write('\n');
        return null;
    });

    return env;
}
```

```
import console;

func main() {
    console.print("Hello World");
}
```