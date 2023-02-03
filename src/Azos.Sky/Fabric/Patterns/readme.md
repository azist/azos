# Fabric Patterns

This namespaces contains cloud design patterns based on Fabric: they provide higher-level
abstractions which are based on lower-level `Fiber` primitives

- **Workflow** - provides a skeleton for a multi-phased supervisor process comprised of supervised work items
- **DebouncedAction** - an action which buffers multiple events (e.g. customer email sends) together to reduce eventual 
  activity frequency