# list
**Lists** are ordered and mutable. You can append, remove, or index elements.

---

## Creating lists
```none
// Examples:
var l = [1, 2, "Hello", null];
var empty = [];
```

---

## Properties

| Property   | Description                     | Example              |
|------------|---------------------------------|----------------------|
| `.length`  | Returns the number of elements  | `[1, 3, 6].length` → 3   |

---

## Methods

| Method             | Description                          | Example                            |
|--------------------|--------------------------------------|------------------------------------|
| `append(o)`          | Adds an object to the list                | `l.add("Hi")` → `[..., "Hi"]`          |
| `pop(idx)`          | Removes and returns the object at index `idx`                | `l.pop(1)` → 2          |
| `contains(o)`          | Checks whether the object `o` is in the list     | `l.contains(10)` → false          |

---

## Indexing

```none
var l = [1, 2, "Hello", null];
var second = l[1];
l[3] = -20;
```

---

## Comparison

```none
var l1 = [20, 30];
var l2 = [];

l1 == l2;   // false
l1 == l1;   // true
l2 == l2;   // true

// Non empty list → true
if(l1) {

}

// Empty list → false
if(l2) {

}
```