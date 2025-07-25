# Events
Events are objects that allow you to attach callback functions, which are triggered when the event is invoked.


## Creating events
Events are created using the keyword `event` as an expression. You can invoke an event simply by calling it like a function:
```none
export message = event;

func main() {
    message("Hello World");
}
```

## Handling events
To handle an event, you can define a callback using the `event` keyword again, this time as a declaration.
```none
import console;
import message;

event message(msg) {
    console.print("Received: ", msg);
}
```
When the event `message` is triggered (as shown in the previous example), all callbacks registered for it will be executed with the given arguments.

## Filtering Parameters
You can restrict when a callback is executed by specifying expected values for the parameters. This allows you to respond only to specific cases.
```none
import console;
import message;

event message(msg ["hello", "world"]) {
    console.print("Received special case: ", msg);
}
```
In the example above, the callback is triggered only when `msg` is `"hello"` or `"world"`.