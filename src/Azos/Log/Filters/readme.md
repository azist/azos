# Log Filtering

Back to [Documentation Index](/src/documentation-index.md)

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
