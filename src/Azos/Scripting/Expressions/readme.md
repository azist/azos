# Scripting / Expressions

The `Azos.Scripting.Expressions` namespace provides types for building expression trees from 
config sources.
This is used as an ad-hoc scripting in various places, such as advanced log filtering.

An abstract syntax expression tree is formed by injection of appropriate expression node types (such as operators)
from config script vector, consequently you can create complex expressions.

Typically a `BoolFilter<TContext>` is used in various places to pattern match against some conditions,
such as whether to log or not to log specific message, default some business data, test a strategy for a match
under specific business case etc.

One may think about expressions+config as a simple form of **DSL** (Domain Specific Language).



The following example of using such DSL is based around logging; we are configuring an advanced 
log pattern match expression to determine whether particular sink needs to log messages:
```csharp
log
{
  . . .
  sink
  {
    filter-expression
    {
      type="Azos.Log.Filters.Or"
      left
      {
        type="Azos.Log.Filters.ByTopic"
        include="wv*;wave;wave-*-han*"
        exclude="wvSocket"
      }
      right
      {
        type="Azos.Log.Filters.ByFrom"
        include="*wave*"
      }
    }
  }
  . . . 
}
```
