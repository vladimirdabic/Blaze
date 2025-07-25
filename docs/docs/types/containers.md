# Containers

### `string`
Strings are immutable sequences of characters. Common operations include:

* Concatenation: `"Hello " + "world"`
* Indexing: `"abc"[0] → "a"`
* Length: `"abc".length → 3`

> See [Strings](strings.md) for more details.

### `list`
Lists are ordered and mutable. You can append, remove, or index elements:
```none
var items = [1, 2, 3];
items.append(4);
console.print(items[0]); // 1
```

> See [Lists](lists.md) for more details.

### `dict`
Dictionaries store key-value pairs. Keys are usually strings:
```none
var user = {
    "name": "Alice",
    "age": 30
};

console.print(user["name"]);
```

> See [Dictionaries](dicts.md) for more details.