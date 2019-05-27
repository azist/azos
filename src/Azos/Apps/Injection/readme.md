# Dependency Injection
Back to [Documentation Index](/src/documentation-index.md)

Azos provides a built-in class field dependency injector accessible via `IApplication.DependecyInjector`
property. Azos **system servers** (Mvc controllers, Glue servers) **invoke DI mechanisms
automatically**, so by the time action methods are called, the controller/server has already been injected into.

The default DI injector injects application-chassis-rooted objects, components such as: app, log, data store, glue, modules, etc.
This is done on purpose to establish a hierarchical application component-based architecture
and limit the frivolous allocation of components and their inter-dependencies. **The components should "talk" via their director.**
See [Azos Application](/src/Azos/Apps) overview.


## How to Inject
When developing application code, you decorate private fields in you business domain classes with `[Inject]` attributes. 
The fields **get injected upon invocation** of `IApplicationDependencyInjector` on the instance being injected-into: 
`App.DependencyInjector.InjectInto( myInstance )`. This is handled automatically by the framework for server modules (Glue and Mvc), 
so one rarely (if ever) needs to call DI service manually.

>**Why field injection** specifically, and not property injector? - this is because until C# 7.3
there is no way to target backing field for auto-properties attribution, the `[field:NonSerialized] public X{get;set;}`
was not available on prior versions of C# compiler., furthermore setting private fields is no different than what is 
typically used in constructor DI for testing.

You can also implement a custom injection code using [`IApplicationInjection`](/src/Azos/Apps/Injection/IApplicationDependencyInjector.cs)
interface on your instance, which will be called by an injector.

You can create a constructor with dependency parameters for testing. This constructor will be used just to manually allocate
objects in your unit tests. This is not a requirement of the framework, as you can use DI in unit testing as easily
(invoke DI in unit tests instead of adding a special constructor). Note: *some developers argue that adding a constructor is
easier for understanding of what dependencies are needed for test*

>Fields decorated with `[Inject]`*(and its derivatives)* are excused from binary Slim and JSON serialization as they are
>transitive dependencies. Consequently, these fields are not deserialized back, for example while working with Pile cache
>or object store. The injected dependencies are also lost during inter-process teleportation as different process has
>different dependency sources. Use ["DI Extension Method Pattern"](#di-extension-method-pattern) described below 

## Inject What
You can inject whatever component is supported by the injector. The default injector supports objects rooted at the app chassis.
This is purposely done to limit the "choices" and preclude chaotic object inter-dependencies.

The default injector [`ApplicationDependencyInjector`](/src/Azos/Apps/Injection/ApplicationDependencyInjector.cs) interprets 
[`InjectAttribute`](/src/Azos/Apps/Injection/Attributes.cs) on all decorated fields. Inject attribute `Apply()` method does probing: 
it takes the field declaration type or `Type` parameter specified in the constructor and sees if it is an `IModule` then it tries to get
module by type, then by name, then inspects app root objects.

There are a few derivatives of `InjectAttribute` that do not do the full probing:
- **InjectModuleAttribute** - injects modules only of `App.ModuleRoot` property (used: `[InjectModule] IMyModule module;`)
- **InjectSingletonAttribute** - injects app singleton instance by type(used: `[InjectSingleton] MyProcess process;`)

The following app root objects are supported:
```CSharp
//ApplicationDependencyInjector.cs
/// <summary>
/// Enumerates app injectable roots (root application chassis objects).
/// This method is usually used by [Inject]-derived attributes for defaults
/// </summary>
public virtual IEnumerable<object> GetApplicationRoots()
{
    yield return App;
    yield return App.Log;
    yield return App.DataStore;
    yield return App.Instrumentation;
    yield return App.SecurityManager;
    yield return App.Random;
    yield return App.TimeSource;
    yield return App.Glue;
    yield return App.ObjectStore;
    yield return App.EventTimer;
}
```

The dependency injection is performed into private fields decorated with the `[Inject]` attribute. Once injected,
you can references dependencies in the `Validate()`(and any other) method. In our example we invoke validation
routine which checks if the specified email is used by some other patient ID - this is an example of module injection.
Modules are loaded during app boot (by default), and can be resolved via `App.ModuleRoot.Get<IModule>(name)` service locator,
by code. The DI injects those fields for you so you do not need to use Service location.

To activate DI, one must use the following [`IApplicationDependencyInjector`](/src/Azos/Apps/Injection/IApplicationDependencyInjector.cs),
which is accessible via `App.DependencyInjection` property:
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

Azos applications provide default implementation based on [ApplicationDependencyInjector.cs](/src/Azos/Apps/Injection/ApplicationDependencyInjector.cs).
This implementation uses reflection and injects dependencies rooted at IApplication. You can certainly create a specific injector
and inject something else but this is not needed in 99% of cases. Azos purposely roots business logic, logging, and data access of the 
central application chassis so the design is very simple, testable and efficient.


## Example

As an example, let us consider the following domain entity (a model) representing `Patient` data:
```CSharp
[Table("tbl_patient")]
public Patient : TypedDoc
{
  private Patient(){}

  //This .ctor is used for testing injection; it is not mandatory
  public Patient(ILog log, IPatientValidationModule validation)
  {
    m_Log = log;
    m_Validation = validation;
  }

  [Inject] ILog m_Log;
  [Inject] IPatientValidationModule m_Validation;

  [Field(required: true, maxSize: Domains.ID_MAX_SZ)]
  public string PatientID { get; set; }
 
  [Field(required: true, kind: DataKind.EMail, maxSize: Domains.EMAIL_MAX_SZ)] 
  public string EMail { get; set; }

.....

  public override Exception Validate(string target = null)
  {
    var error = base.Validate(target);
    if (error!=null) return error;

    var emailAlreadyUsed = m_Validation.CheckPrimaryEMailUseByAnother(this.PatientID, this.EMail);
    if (emailAlreadyUsed)
    {
      this.LogWarning(m_Log, "Duplicate email");
      return new FieldValidationException(nameof(Patient), nameof(EMail), "The specified Email is already in use");
    }

    return null;
  }
}
```
The class is a data document deriving from `TypedDoc` so Azos CRUD data stores know how to auto generate SQL and map
DB data into this model automatically, also Azos Wave Mvc model binder knows how to take client-side POST and supply
the model instance filled with data into controller body. We will see how this done a bit later.

`Patient` model has two constructors: one private parameterless which is used by de-serializers and binders, it is not
technically needed but it is good pattern for possible ctor chaining; the second constructor takes the "complex" signature
and will not be auto-invoked by Azos automatically. Azos does not support auto constructor injection on purpose 
(see philosophy/problems with general purpose DI), so the 2nd ctor is only used for unit testing and is not mandatory to
 have for DI to operate.


Lets consider a microservice server module implemented using [Glue (Azos microservice RPC framework)](/src/Azos/Glue):
```CSharp
  [Glued]
  public interface IPatientRepository
  {
    Patient GetByID(string id);
    void AdmitNew(Patient patient);
  }
....
public class PatientRepositoryServer : IPatientRepository
{
  //this ctor is used for unit testing (not required)
  public PatientRepositoryServer(IApplication app)  => m_DataStore = (m_App = app).DataStore;

  [Inject] IApplication m_App;
  [Inject] IMyAppDataStore m_DataStore;

  public Patient GetById(string id)
  {
    return m_DataStore.Clinical.Fetch<Patient>(id);
  }
  
  public void AdmitNew(Patient patient)
  {
    var error = patient.Validate(m_App);//context/DI injection
    if (error!=null) { ..... throw error; ....}
    m_DataStore.Clinical.Save(patient);
  }
}
```
The DI into the server module is done automatically by the Glue framework which activates the server module. Pay attention to the payload type,
the method takes `Patient` model from the client, and it needs some context internally for validation. We invoke `Validate(IApplication)`
extension method (DI Extension Method Pattern) which performs the dependency injection and calls `Validate()` on the model. Notice the constructor which is used for
unit testing of the Glue server module.

## DI Extension Method Pattern
Business domain objects may need to introduce specific method logic. Those methods would probably need various dependencies.
DI Extension method pattern is used to extend DI functionality into custom methods. This approach allows to keep **`IApplication` context
as an ambient parameter** instead of passing it around various methods which is inconvenient.

Consider `document.Validate()` method described above. When invoked it has to ensure that context is injected. Azos has a built-in extension method for data
document validation:
```CSharp
 // Azos/DataUtils.cs:
 /// <summary>
 /// Perform app context injection and calls Validate() on a data Doc
 /// </summary>
 public static Exception Validate(this Doc doc, IApplication app, string targetName = null)
    => app.NonNull(nameof(app)).InjectInto(doc.NonNull(nameof(doc))).Validate(targetName);

 // Azos/CoreUtils.cs:
 /// <summary>
 /// Shortcut to App.DependencyInjector.InjectInto(...)
 /// </summary>
 public static T InjectInto<T>(this IApplication app, T target) where T : class
 {
    app.NonNull(nameof(app)).DependencyInjector.InjectInto(target);
    return target;
 }
```
Now the method is used like so:
```CSharp
... Mvc Controller ...
  [Inject] IMyDataStore m_DataStore;// injected by Mvc handler

  [ActionOnPost]
  public object AdmitNew(Patient patient)
  {
   //context/DI injection via extension method
    var error = patient.Validate(m_DataStore.App); //we have created a validation in the application scope method
                                                   //keeping the injection as an ambient process 
    if (error!=null) return error;
    m_DataStore.Clinical.Save(patient);
    return new Redirect(URIS.Home);
  }
```
You can create extensions for your particular needs as easily.






---
Back to [Documentation Index](/src/documentation-index.md)

External resources:
- [Service Location (Wikipedia)](https://en.wikipedia.org/wiki/Service_locator_pattern)
- [Dependency Injection (Wikipedia)](https://en.wikipedia.org/wiki/Dependency_injection)
- [IoC with Service Location / Dependency Injection by Martin Fowler](https://martinfowler.com/articles/injection.html)
- [Composition Root Pattern by Mark Seemann](http://blog.ploeh.dk/2011/07/28/CompositionRoot/)

