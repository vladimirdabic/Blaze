# Functions
As we've seen in the previous sections, functions are declared using the `func` keyword.
```none
func my_func(arg1, arg2) {
    // code
}
```
Functions are first-class values in Blaze, meaning they can be assigned to variables, passed as arguments, or returned from other functions.

You can also define functions as expressions (lambdas), for example:
```none
import console;

func main() {
    var my_func = func(arg1, arg2) {
        console.print("Called with: ", arg1, " ", arg2);
    };

    my_func(20, "aaa");
}
```

## Module Functions

Module-level functions are declared at the top scope of a Blaze source file. Like module variables, functions can be exposed and requested from other modules.

The following modifiers can be used with module functions:

* `public`: Exposes the function to other modules
* `private`: The function is accessible only within its own module

If no modifier is provided, `private` is assumed by default.

Here's an example of a module that exposes two functions:
```none
var count = 0;

public func next() {
    return count++;
}

public func current() {
    return count;
}
```
In another module, the exposed functions can be accessed as follows:
```none
import console;
import next;
import current;

func main() {
    console.print(current());
    next();
    next();
    console.print(current());
}
```

## Initialization
Internally, module-level functions are treated as function objects stored in module variables. That means the following declaration:
```none
public func my_func() {
    // code
}
```
Is internally equivalent to:
```none
public var my_func;
static my_func = func() {
    // code
};
```
This ensures consistency with how module variables are initialized and makes the language easier to reason about.