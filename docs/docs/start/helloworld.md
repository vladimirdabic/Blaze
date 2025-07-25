# First program in Blaze

To write a simple hello world program, create a file (eg. `hw.blz`) and write the following code:
```none
extern var console;

func main() {
    console.print("Hello World");
}
```

Now we use `blzc` to compile the source file to a module file:
```none
> blzc -s hw.blz
  Compiled to 'hw.blzm'
```

Now that we have a module file, we can run it using `blzi`:
```none
> blzi -m hw.blzm
  Hello World
```

This is a simple hello world program. Notice that we have to specify that the console variable is external.
Blaze doesn't have a concept of a global environment like most embeddable/scripting languages do.

Blaze works with modules.
Each Blaze source file is compiled to a module, which contains functions, classes and variables.
Variables can be declared as public, private or extern.

The console interface is provided from an external library that contains a module.

During runtime each module can have multiple children and a parent which make up a hierarchy.