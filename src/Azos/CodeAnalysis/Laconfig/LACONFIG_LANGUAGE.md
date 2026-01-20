# Laconfig Language Definition

Laconfig (Laconic Configuration) is a configuration language used in the Azos framework. It is designed to be concise, human-readable, and easy to parse, supporting hierarchical sections, attributes, and data typing via literals.

## 1. Lexical Structure

The source text is encoded in Unicode. The lexical analyzer breaks the source text into tokens.

### 1.1. Whitespace
Whitespace is used to separate tokens and is otherwise ignored.
*   Space ` ` (0x20)
*   Tab `\t` (0x09)
*   Carriage Return `\r` (0x0D)
*   Line Feed `\n` (0x0A)

### 1.2. Comments
Comments are ignored by the parser.
*   **Single-line**: Starts with `//` and extends to the end of the line.
*   **Block**: Starts with `/*` and ends with `*/`. Standard C-style block comments.
*   **Alternative Block**: Starts with `|*` and ends with `*|`. This unique style allows commenting out blocks of code that may already contain standard `/* ... */` comments.

### 1.3. Directives
*   A line (fresh line) starting with `#` as the first non-whitespace character is treated as a **Directive**.
*   Directives are tokenized as `tDirective` but are generally treated as comments/metadata by the standard parser.
*   Example: `#pragma warning disable`

### 1.4. Identifiers
Identifiers are sequences of characters that represent names or unquoted string values.
*   An identifier is any sequence of characters that does not start with a string delimiter and does not contain:
    *   Whitespace
    *   Structural symbols: `{`, `}`, `=`
    *   Comment start sequences: `//`, `/*`, `|*`
    *   String start sequences: `"`, `'`, `$"` , `$'`
*   Allowed characters include `.` `-` `:` `[` `]` `(` `)` `\` etc., as long as they don't trigger the exclusions above.
*   Examples: `log-root`, `section_1`, `c:\path\to\file`, `My.Type,Assembly`.

### 1.5. Literals

#### 1.5.1. String Literals
Strings represent text data.
*   **Regular Strings**:
    *   Enclosed in double quotes `"` or single quotes `'`.
    *   Cannot span multiple lines (newlines are not allowed inside).
    *   Support C#-style escape sequences: `\'`, `\"`, `\\`, `\0`, `\a`, `\b`, `\f`, `\n`, `\r`, `\t`, `\v`, `\uXXXX`, `\x...`.
    *   Example: `"Line 1\nLine 2"` or `'It\'s a string'`.
*   **Verbatim Strings (Multiline)**:
    *   Prefix with `$`. Note: This syntax denotes **verbatim** strings in Laconfig, matching the visual style of interpolated strings in C#, but acts as C# `@` verbatim strings.
    *   Enclosed in `"` or `'` after the `$`. i.e., `$"..."` or `$'...'`.
    *   Can span multiple lines.
    *   Escape sequences (like `\n`) are **NOT** processed; content is taken literally.
    *   Quote escaping: Double the quote character to include it.
        *   In `$"..."`, use `""` for `"`.
        *   In `$'...'`, use `''` for `'`.
    *   Example:
        ```laconfig
        $"This is a
        multi-line string with ""quotes"" inside"
        ```

#### 1.5.2. Null Literal
*   The keyword `null` is recognized as a specific token `tNull`.
*   It represents a null value in the configuration tree.

### 1.6. Structural Symbols
*   `{` : Brace Open (Start of Section)
*   `}` : Brace Close (End of Section)
*   `=` : Equality (Assignment of Value)

---

## 2. Syntactic Grammar

The grammar is defined recursively.

### 2.1. Compilation Unit
A Laconfig document consists of a single Root Section.

```bnf
Configuration ::= RootSection [EOF]
```

### 2.2. Root Section
The root section must have a name, an optional value, and a body.

```bnf
RootSection ::= Identifier_Or_String [ "=" Value ] "{" Content "}"
```

### 2.3. Section
A section represents a node in the configuration tree.

```bnf
Section ::= Identifier_Or_String [ "=" Value ] "{" Content "}"
```

*   **Header**: Starts with a name (Identifier or String).
*   **Optional Value**: Can have a value assigned using `=`.
*   **Body**: Must be enclosed in `{` and `}`.

### 2.4. Attribute
An attribute is a leaf node with a value.

```bnf
Attribute ::= Identifier_Or_String "=" Value
```

*   Attributes consist of a name, an `=` sign, and a value.
*   Attributes do **not** have children (no `{}`).

### 2.5. Elements (Common Productions)

**Identifier_Or_String**:
```bnf
Identifier_Or_String ::= tIdentifier | tStringLiteral
```

**Value**:
```bnf
Value ::= tIdentifier | tStringLiteral | tNull
```
*   Note: Since identifiers can contain many characters, values like `true`, `123`, `enum-value` are parsed as identifiers and stored as strings/atoms in the configuration. Only `null` and quoted strings are distinct types at the parser level.

**Content**:
```bnf
Content ::= ( Section | Attribute )*
```
*   The body of a section contains a list of mixed Sections and Attributes.

### 2.6. Parsing Logic (Disambiguation)
The parser disambiguates between a Section and an Attribute by looking ahead for the `{` symbol.

Sequence for an entry inside `Content`:
1.  **Read Name** (`Identifier` or `String`).
2.  **Check for `=`**.
    *   If present: **Read Value** (`Identifier`, `String`, or `Null`).
3.  **Check for `{`**.
    *   If present: The entry is a **Section**. (It may or may not have had a value in step 2). Recursively parse `Content`.
    *   If absent:
        *   If a Value was read in step 2: The entry is an **Attribute**.
        *   If no Value was read: **Syntax Error** (An entry cannot just be a name; it must be a Section `{...}` or an Attribute `= val`).

### 2.7. Examples

**Basic Structure**:
```laconfig
app
{
  log-level = debug   // Attribute
  
  database            // Section (null value)
  {
     connection = "mongo://localhost"
  }
}
```

**Section with Value**:
A section can hold a value *and* children.
```laconfig
logger = file         // Section has value "file"
{
  path = "c:\logs"    // And child nodes
}
```

**Verbatim Strings & Comments**:
```laconfig
root
{
  # This is a directive (ignored)
  |* This is a 
     block comment *|
     
  script = $"
     function valid() {
       return true;
     }
  "
}
```

## 3. Second layer parsing

Laconfig parsers typically produce a tree of `LaconicConfiguration` nodes. The second layer of parsing involves interpreting the string values stored in these nodes into 
specific data types (e.g., integers, booleans, enums) as required by the application logic. This is done using helper methods provided by the Azos framework, 
such as `AsInt()`, `AsBool()`, etc.

An important part of the second layer parsing is expansion of environment variables and other dynamic content within string values.
This is really NOT specific to Laconfig format but rather a common logic applied to any string-based configuration format in Azos.

### 3.1 Variable expansion

Variables start with `$` and are enclosed in `()`. For example, `$(path)` will be replaced with the value of the path evaluation.

> Syntax highlighters should use a bit different color for `$(...)` patterns inside Laconfig string literals to denote that these are special constructs.


### 3.1. Environment Variable Expansion
Environment variables are special kind of variables described above having their name start with a `~` prefix, e.g. `$(~HOME)`. 
During second layer parsing, these variables are replaced with the corresponding environment variable values.

## 4. Keywords
The configuration format does not have any specific keywords defined, however the syntax highlighters may chose to highlight the following identifier tokens
as keywords:

- `name`
- `type`





