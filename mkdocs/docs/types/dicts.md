# dict
**Dictionaries** store key-value pairs.

---

## Creating dicts
```none
var user = {
    "name": "Alice",
    "age": 30
};
```

---

## Properties

| Property   | Description                     | Example              |
|------------|---------------------------------|----------------------|
| `.length`  | Returns the number of key-value pairs  | `user.length` → 2   |
| `.keys`    | Returns a list of keys | `user.keys` → `["name", "age"]` |

---

## Methods

| Method             | Description                          | Example                            |
|--------------------|--------------------------------------|------------------------------------|
| `get(key, default)` | Returns the value of key `key` if it exists, otherwise `default`                 | `user.get("points", 0)` → 0          |
| `contains(key)`          | Checks whether the key `key` exists     | `user.contains("age")` → true          |

---

## Indexing

```none
var user = {
    "name": "Alice",
    "age": 30
};

var name = user["name"];
user["age"] = 21;
```

---

## Comparison

```none
var d1 = {};
var d2 = {};

d1 == d2;  // false
d1 == d1;  // true
d2 == d2;  // true
```