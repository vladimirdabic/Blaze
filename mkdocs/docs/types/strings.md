# string
**Strings** are immutable sequences of characters used to store text.

---

## Creating strings
```none
// Examples:
var s = "hello";
var empty = "";
```

---

## Properties

| Property   | Description                     | Example              |
|------------|---------------------------------|----------------------|
| `.length`  | Returns the number of elements  | `"abc".length` → 3   |

---

## Methods

| Method             | Description                          | Example                            |
|--------------------|--------------------------------------|------------------------------------|
| `split(delim)`          | Splits a string with a delimiter                | `"a,b,c".split(",")` → `["a", "b", "c"]`          |

---

## Indexing

```none
var s = "hello";
var first = s[0]; // 'h'
```

---

## Comparison

```none
"abc" == "abc"   // true
"abc" != "def"   // true
```