# Classes
Classes are declared using the `class` keyword.
```none
class Counter {
    var current;

    Counter(initial) {
        if(initial == null)
            current = 0;
        else
            current = initial;
    }

    func next() {
        return current++;
    }
}
```
Classes are also first-class values, meaning they can be assigned to variables, passed as arguments, or returned from functions.

!!! note
    Inside class functions, you can use `this` to refer to the current instance and access its members, e.g., `this.current`.

## Instantiating
To create an instance of a class, use the `new` keyword:
```none
func main() {
    var c1 = new Counter(5);
    var c2 = new Counter();
    
    console.print(c1.next());
    console.print(c1.next());
    console.print(c2.next());
    console.print(c2.next());
}
```


## Class Scope and Visibility
Unlike functions and variables, all classes are module-level declarations. They can only be declared at the top scope of a Blaze source file.

The following modifiers can be used with classes:

* `public`: Exposes the class to other modules
* `private`: The class is accessible only within its own module

If no modifier is provided, `private` is assumed.

!!! note
    All class members (variables and functions) are implicitly public. Blaze does not currently support member-level access modifiers.