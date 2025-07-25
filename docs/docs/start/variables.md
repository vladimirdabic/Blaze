# Variables

Variables in Blaze are declared using the `var` keyword:
```none
import console;

func main() {
    var name = console.input("What's your name? ");
    console.print("Hello ", name, "!");
}
```

## Module Variables

Module-level variables are declared at the top scope of a Blaze source file. These variables play an important role in Blaze's modular design. Since each Blaze source file is compiled as a module, variables can be:

* Exposed to other modules
* Requested from other modules

The following modifiers can be used with module variables:

* `public`: Exposes the variable to other modules
* `private`: The variable is accessible only within its own module
* `extern`: Indicates the variable is defined in another module

If no modifier is provided, `private` is assumed by default.

```none
var some_var = 20;
public var username = "vladimirdabic";
extern var console;

func main() {
    console.print(username);
}
```

!!! note
    `public var` and `extern var` can be written in a short way using the `export` and `import` aliases, respectively.

```none
var some_var = 20;
export username = "vladimirdabic";
import console;

func main() {
    console.print(username);
}
```

## Initialization
When a module is loaded, all module-level variables are initialized. This is done by what Blaze calls a *static function*.

The static function is a special block of code that runs automatically when the module is loaded, similar to a constructor in a class. It's used to assign initial values or perform setup tasks.

You can define static code using the `static` keyword, which can be followed by either a single statement or a block of statements:
```none
static var x = 20;

static {
    // code
}
```

For example, the following module variable declaration:
```none
var x = 20;
```
Is internally equivalent to:
```none
var x;
static x = 20;
```
This means module variables are declared first, and then initialized when the module is loaded.