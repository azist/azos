# Configuration Forest

(AZ#562)

Forest is a version-controlled (like Git) repository of named hierarchies called trees of structured configuration files having
their content attributes and sections inherited from the higher tree hierarchy levels.
These config files are identified by a full "node path" which are a kin to file system files.

> **Config Forest** may be described as a distributed version-controlled file system of structured config files with file content inheritance capabilities.

Forest is useful for managing business and system taxonomies/classification trees of various kinds, 
e.g. a regional catalog of entities, or risk classifications:

```
region://us/east/cle/solon/maple-road
risk://health/smoking
risk://health/activity
```

The forest is a collection of named trees, each having a unique name in a forest. The **name of a tree is an `Atom`**, meaning:
- 1 to 8 ASCII only characters
- 'a'...'z' or 'A'..'Z'
- '0'..'9'
- '-' or '_' are the only allowed separators

The names of nodes in a tree are NOT as constrained:
- Tree name (an Atom) is separated from node path using '://' delimiter
- Nodes of the tree path are delimited by a single '/' (forward slash)
- A segment of path has to be from 1 to 64 Unicode Characters (no ASCII limit)
- All segment symbols are permitted except for '/' (forward slash)
- Forward slash character may be escaped with '//' double forward slash (this rule does NOT apply to tree name separator)

> A tree may have a **maximum design depth of 0xff levels**.

> **Tree paths are CASE-INSENSITIVE!!!**

Every node has a tree-unique GDID. The trees are completely isolated and in fact could have a separate dedicated storage handler/engine
which handles requests into that tree.

A tree node gets addressed either by PATH or GDID within a tree.
