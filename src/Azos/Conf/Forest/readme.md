# Configuration Forest

(AZ#562)

Forest is a version-controlled (like Git) repository of named hierarchies called trees of structured configuration files having
their content attributes and sections inherited from the higher tree hierarchy levels.
These config files are identified by a full "node path" which are a kin to file system files.

> **Config Forest** may be described as a distributed version-controlled file system of structured config files with file content inheritance capabilities.

Forest is useful for managing business and system taxonomies/classification trees of various kinds, 
e.g. a regional catalog of entities, or risk classifications:

```
  //Tree root + (AsOf) date supplied in the function call
  region@geo::/
  region.path@geo::/

  //Get node by path + (AsOf) date supplied in the function call
  region.path@geo::us/east/cle/solon/maple-road

  //Get node by ID
  region.gnode@geo::3:11:790163

  //Get node of specific version
  region.gver@geo::2:5:980

  risk@med::health/smoking
  risk@med::health/diabetes/carbs
```

A tree node gets addressed either by PATH or GDID within a tree via an `EntityId` with either `gnode` or `path`
address schema. The `path` is a default schema assumed if omitted.

The forest is a collection of named trees each denoted by `EntityId.Type`. 
Forest uses `EntityId`, therefore the type is an `Atom`, hence it has to comply with the following rules:
- 1 to 8 ASCII-only characters
- 'a'...'z' or 'A'..'Z'
- '0'..'9'
- '-' or '_' are the only allowed separators

The names of nodes in a tree are NOT as constrained:
- Tree type (an Atom) is contained in `EntityId.Type`
- Tree node path (a string) is contained in `EntityId.Address`
- Nodes of the tree path (`EntityId.Address`) are delimited by a single '/' (forward slash)
- A segment of path has to be anywhere from 1 to 64 Unicode Characters (no ASCII limit)
- All segment symbols are permitted except for '/' (forward slash)
- Forward slash character may be encoded as '%2f' similarly to URI encoding with `%`
- The `%` character may be encoded as `%25`

> A tree may have a **maximum design depth(path length) of 255 levels**.

> **Tree paths are CASE-INSENSITIVE!!!** and are always lower-case

Every node has a tree-unique GDID. The trees are completely isolated and in fact could have a separate dedicated 
storage handler/engine with its own rule set which handles requests for that specific tree.



