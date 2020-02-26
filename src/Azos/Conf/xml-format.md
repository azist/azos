# Xml Config Format

Back to [Documentation Index](/src/documentation-index.md)

## Overview

> **NOTE:** Xml is considered to be obsolete config format and is being gradually phased-out. Use Json instead if you need to persist config vectors in data stores and interop

```xml
  <!-- only block comments -->
  <root a="1" b="2" c="&#10; verbatim &#10; strings ">
   -900
   <sub z="my\nmessage! &quot;OK&quot;">
   </sub>
  </root>
```
XML is very similar to JSON in its limitations. It is verbose, does not have line comments (which is very inconvenient) and requires additional escape sequences. In practice this format should only be used for persisting config snippets in DBs and other external systems.



See also:
- [Configuration Overview](readme.md)
- [Json Config Format](json-format.md)
- [Laconic Config Format](laconic-format.md)
- [Command Args Format](cmdarg-format.md)

Back to [Documentation Index](/src/documentation-index.md)


