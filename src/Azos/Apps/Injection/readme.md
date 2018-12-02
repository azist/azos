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







## Problems with General-Purpose Object Allocation in Business Apps

**TLDR**
>Business apps are nothing more but manipulators of model data usually supplied by Http post, validated, transformed
>and stored into a DB store (which usually uses auto-generated SQL/CRUD). The complexity of classes/inheritance and object graph
>allocation is mostly needed in system code (such as Azos framework itself), not a business code (such as XYZ system built on Azos)
>as most business code is procedural/linear logic operating on business-models. Most entities come into business code already pre-allocated by
>the system (controllers, service modules, entity mapping, model bindings)

DI is good, in general, what is not good is that in the hands of purists and careless developers general purpose DI availability creates 
a temptation to create unneeded classes and allocate too many objects for tasks which **do not need object instances in principle**. 

For example, the following lists objects that need to get allocated in a typical app:

- App component graph - allocated by the system at start (system code)
- Logging - allocated at app start (system code)
- Instrumentation - allocated at app start (system code)
- Data Store/Access Repository -  allocated at app start (system code)
- Security Manager - allocated at app start (system code)
- Domain-specific business logic - modules allocated at app start (system code)
- Web call POST/PUT payload -> Domain model - allocated by MVC controller/model binder (system code)
- Data Access model - allocated by datastore/access layer (system code)
- Custom data structures/Nodes - **allocated by user code**
- Domain model object - allocated by datastore/object mapper (system code mostly) **and user code**
- Deserialization: stream-> object: - allocated by serializer (system code)

As we can see in the list above, **most of the object allocations are done by the system(framework)** code. Business logic 
developers should concentrate on implementing:

- Domain models - objects that reflect business (e.g. User, Account, Doctor etc...)
- Domain logic - objects that contain business rules. The practice has shown that the majority of business logic is linear and procedural, hence it makes sense to put those functions in "logic modules" that get loaded in the app context and used as a "service"

**Business logic is mostly procedural in its core**, and it takes "business models", validates them and transacts
the db/store/queue. A business-oriented data driven application is all about good UI front end and matching backend that
takes data (usually via web API facade), binds it to server models, validates, transforms and stores those models either
directly into the store or via some kind of queue/CQRS/event sourcing etc.

It became apparent that **frivolous object allocations should NOT be encouraged** altogether in business logic.

While writing software for an end-user problem, If you are spending too much time doing system architecture (inventing non-domain classes) on the
server side - then there is something wrong with your framework which you base your architecture on.
**Business app must ONLY contain business domain objects.**




---
Back to [Documentation Index](/src/documentation-index.md)