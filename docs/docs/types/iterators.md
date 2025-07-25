# iterator
**Iterators** produce values lazily.

---

## Creating iterators
```none
// Examples:
var s = "hello";
var it = iter s;

// Creates an iterator internally
for(var c : s) {
    // ...
}
```

---

## Properties

| Property   | Description                     | Example              |
|------------|---------------------------------|----------------------|
| `.available`  | Returns whether the iterator has more values to produce  | `it.available` → true   |
| `.next` | Returns the next value and advances the iterator | `it.next` → h |

---

## Comparison

```none
var l = [1, 2, 4];
var s = "hello";

var it1 = iter l;
var it2 = iter s;

it1 == it2;  // false
it1 == it1;  // true
it2 == it2;  // true
```