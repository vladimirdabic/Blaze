# Core Types Overview

Blaze provides several built-in types that form the foundation of the language. This page gives a brief overview of each type, their literal syntax, mutability, and common operations.

| Type         | Example Literals           | Mutable? | Description                        |
|--------------|----------------------------|----------|------------------------------------|
| `null`       | `null`                     | —        | Represents the absence of a value  |
| `boolean`    | `true`, `false`            | No       | Boolean true/false values          |
| `number`     | `42`, `3.14`, `-1`         | No       | Integer and floating-point numbers |
| `string`     | `"hello"`, `""`            | No       | Text values (UTF-8)                |
| `list`       | `[1, 2, 3]`, `[]`          | Yes      | Ordered collection of values       |
| `dict`       | `{"key": 10}`, `{}`        | Yes      | Key-value mapping                  |
| `function`   | `func(x) { ... }`          | —        | First-class functions              |
| `class`      | `class C { ... }`          | —        | Blueprint for objects              |
| `object`     | `new C()`                  | Yes      | Instance of a class                |
| `iterator`   | `iter [1, 2, 3]`             | Yes      | Produces values one at a time      |

!!! note
    Types like `function`, `class`, and `iterator` are also first-class values, meaning they can be passed to or returned from functions, stored in variables, and compared.