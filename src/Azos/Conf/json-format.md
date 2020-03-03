# Json Config Format

Back to [Documentation Index](/src/documentation-index.md)

## Overview

```json
{
  "root": {
  "-section-value": "-900",
  "a": "1",
  "b": "2",
  "sub":{
    "z": "my\nmessage! \"OK\""
  }
  "c": "\n verbatim \n strings "
 }
}
```

The only advantage of JSON over Laconic is if you need to read it somewhere else which rarely happens, e.g. if you need to read in in a SQL query. JSON is used for cases when config vector is stored in DBs or sent to client. Other than that, JSON is much harder to use because it does not support comments (classic json does not), and has much longer footprint/more typing and extra quotes.

Consider the difference:
```csharp
 a=1{b=one{} b=two{} b=three{} c{atr=val}}

 - vs -

 {"a":{"-section-value": "1","b":[{"-section-value": "one"},{"-section-value": "two"},{"-section-value": "three"}],"c":{"atr":"val"} }}

```



See also:
- [Configuration Overview](readme.md)
- [Laconic Config Format](laconic-format.md)
- [Xml Config Format](xml-format.md)
- [Command Args Format](cmdarg-format.md)

Back to [Documentation Index](/src/documentation-index.md)


