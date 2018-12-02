# Dependency Injection
Back to [Documentation Index](/src/documentation-index.md)

Azos provides a built-in class field injector accessible via `IApplication.DependecyInjector`
service.

Contract [`IApplicationDependencyInjector`](/src/Azos/Apps/Injection/IApplicationDependencyInjector.cs):
```CSharp
 /// <summary>
 /// Designates entities that process [Inject]-decorated fields and IApplicationInjection on
 /// a target objects passed into InjectInto(target) method.
 /// Access application default injector using IApplication.DependencyInjector property
 /// </summary>
 public interface IApplicationDependencyInjector : IApplicationComponent
 {
  /// <summary>
  /// Injects application context along with its derivatives (e.g. modules, logs etc.) as
  /// specified by the field-level attribute decorations on a target instance
  /// </summary>
  void InjectInto(object target);
...
}
```

Azos applications provide default implementation based on [ApplicationDependencyInjector.cs](/src/Azos/Apps/Injection/ApplicationDependencyInjector.cs)


TBContinued........................
...............
............
----
----











---
Back to [Documentation Index](/src/documentation-index.md)