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
    -d              Compiles as a debug module

Usage: blzi.exe [options]
Options:
    -m [file]       Module file to execute
    -d              Print the contents of the module file
```

## Example
```
extern var print;

func main() {
    print("Hello World");
}
```
```
> .\blzc.exe -s hw.blz
  Compiled to 'hw.blzm'
> .\blzi.exe -m hw.blzm
  Hello World
```
This is a simple hello world program. Notice that we have to specify that the print variable is external.\
Blaze doesn't have a concept of a *global environment* like most embeddable/scripting languages do.

Blaze works with **_modules_**.\
Each Blaze source file is compiled to a *module*, which contains functions, classes and variables.\
Variables can be declared as public, private or extern.

The print function is provided from an internal module in the interpreter.\
That might sound like a global environment, but unless you specify that print is *extern* the interpreter won't be able to see it.

During runtime each module can have multiple children and a parent which make up a hierarchy.

## Libraries
Blaze doesn't come with a standard library, that task is up to the developer who's embedding the language into their application.\
The developer should keep in mind that goal is to provide the user only with functions they'll actually need.

In the future there might be a system where you can load modules that are made with C# and exported as a C# DLL.\
Maybe even a C++ DLL, which would work better in the future if I ever write an interpreter in C++.\
When that's possible, then some kind of standard library might appear.

## Features
Blaze currently supports:
- Basic values (Numbers, Strings, Booleans, null)
- Functions
- Classes
- Lists
- Dictionaries
- All of the expected control flow (if, while, for)

Planned features:
- Events

## Events (to be added)
As Blaze is an embeddable language, it has to have some kind of event system.\
The host application might expose an event to which you can attach a callback.
```
// here the application exposes an object that holds multiple events
extern var UserEvent;
extern var print;

event UserEvent.Connect(username) {
  print(username, " connected");
}

event UserEvent.Disconnect(username) {
  print(username, " disconnected");
}
```
You can attach multiple callbacks to an event, obviously.

If you want something to happen only if an event parameter is a specific value, you can pass a list of expected values.\
If the parameter is not one of these, the callback won't execute.

```
event UserEvent.Connect(["vladimirdabic", "doofusjack"]) {
  print("Specific user connected");
}
```

This is just syntactic sugar for
```
event UserEvent.Connect(username) {
  if(username != "vladimirdabic" && username != "doofusjack") return;

  print("Specific user connected");
}
```
