# Strategy Pattern and Business-Driven IOC

Back to [Documentation Index](/src/documentation-index.md)

Azos framework provides a built-in strategy pattern with dynamic implementor binding (a form of DI for business logic). Strategy pattern is intended to
provide various algorithm implementations per required contract. 
See https://en.wikipedia.org/wiki/Strategy_pattern

For example, you may need to
calculate shipping costs. The costs are dependent on shipping source/destination combinations
and on the shipping type (such as "2nd day air") used by a specific shipping provider (such as FedEx, UPS etc..).

## Strategies are form of Business-Driven DI
Azos strategy functionality is a form of business-driven DI (dependency injection). Unlike the typical
DI which is set uniformly at the application-level, the strategy DI is more business context-based as the
system provides you with a type of strategy implementation, possibly configured with parameters which best suit
the context of a specific call. 

**Modules** are used when implementing application-wide logic, be it system logic (e.g. `IStrategyBinder` is a module) 
or business logic, whereas **Strategies** are used to implement business-specific algorithms, consequently
business logic modules typically rely on various strategies internally to provide their services.

For example, services such as logging use general purpose DI, because logging is used uniformly throughout applications.
On the other hand, you may store strategy DI patterns/parameters in a database containing millions of rows for customers.
You can then use a custom strategy [`BindingHandler`](BindingHandler.cs) which matches best policies for every customer record based
on a custom algorithm match scoring system. The details of matching may be as simple as "give me the first class implementing
ABC contract" vs "give me a class implementing ABC contract which has the highest match score as defined by parameters/formula etc.."


## Strategy Contract and Context
Strategies are declared using two interfaces: `IStrategyContext` and `IStrategy<TContext>`.

[`IStrategyContext`](Intfs.cs) is basically a marker interface which defines an entity supplying parameters for every strategy
call. Contexts define how strategy implementors are matched by binder. Upon implementor instance binding, the context value gets "bound"
to `IStrategy.Context` property.

In the following example we will try to model a shipping rate calculation strategy
```csharp
  interface IShippingContext : IStrategyContext
  {
    ShippingProvider Provider{ get; }
  } 
```

Now we can declare the strategy contract:
```csharp
  interface IShippingPriceCalc : IStrategy<IShippingContext>
  {
    //You can use a property: IShippingContext Context { get; }
    IEnumerable<Rate> GetShippingRates(Location from, Location to, Package what);
  }
```


## Using Strategies

When you need to delegate some work to an algorithm represented by a strategy, you call a `IStrategyBinder` service provided by a module
injected on the app level. It provides a `Bind` method.

The method has such name because it **binds the best implementation of the requested contract type as needed for the specific business use-case as 
stated by the supplied context instance to the context instance**.

```csharp
  /// <summary>
  /// Outlines contract for strategy factories which bind specific strategy contract instances to specific business cases (contexts).
  /// Finds a strategy implementation which satisfies the specified contract type, and is configured in a
  /// way specific for the concrete use-case as supplied via Context property.
  /// Strategies couple context-specific logic along with their configuration parameters.
  /// </summary>
  public interface IStrategyBinder : IModule
  {
    /// <summary>
    /// Gets a strategy instance per the specified contract and binds it to the specified call context.
    /// This method is synchronous because by design it is expected to be CPU-bound and
    /// use cache for performance internally instead of relying on external data store access
    /// </summary>
    TStrategy Bind<TStrategy, TContext>(TContext context) where TStrategy : class, IStrategy<TContext>
                                                          where TContext : IStrategyContext;
  }
```

The method "binds" an instance of the **most appropriate** implementing type for `TStrategy` setting it's `TContext Context` property
to the requested value. The binding is performed using `IStrategyBinder` implementation (e.g. [`DefaultBinder`](DefaultBinder.cs)), 
which in turn may delegate its work further down the line to [`BindingHandler`](BindingHandler.cs).

The determination of what is considered to be "the best match" is handled by the binder, 
which may base its scoring on various aspects. Default `BindingHandler` respects `IPatternStrategyTrait` along with `StrategyPattern` attributes (described below).

The following snippet illustrates the consumption case:
```csharp
  //use standard DI to inject Strategy Binder
  [Inject] IStrategyBinder m_Strategies;
  ...

  //initialize IShippingContext with specific provider
  var ctx = new ShippingContext{ Provider = Shippers.Fedex };

  //ask IStrategyBinder to give us the best implementor of IShippingPriceCalc in the specified context
  var calc = m_Strategies.Bind<IShippingPriceCalc, IShippingContext>(ctx);

  //calculate the rates, passing arguments to strategy method call directly
  var rates = calc.GetRates(request.From, request.To, request.Package);

  ...
```

 **Important design decision** regarding passing strategy arguments as context properties vs strategy-specific method call arguments.
 You should pass as many arguments as possible to strategy methods, **unless** these values are used
 for strategy matching/binding, in which case you should pass it via context. In the shipping example above, the
 "from/to/package" parameters are passed into "GetRates" method, whereas `Provider` is set to "Fedex" at the context level.
 This is because provider affects the implementor type selected by the binder, consequently the `calc.Context` property
 is preset with Fedex provider and can not be changed. In this specific example, you can call `GetRates()` many times with various arguments.
 The determination of how to pass parameters is dependent on specific business case needs.

 **Immutable State Notice:** strategy method implementations should not mutate **strategy state**. The strategy state (such as properties) 
 can only be used as configuration parameters affecting the strategy algorithm execution, but calling strategy algorithms 
 multiple times should produce the same results making a bound strategy instance idempotent in terms of its algorithmic 
 behavior. This is not to say that a strategy may not have any mutating affect on the external state (such as writing to database).
 Strategy state is ephemeral and is lost. Binder is not required to maintain any state and allocates a new instance of 
 implementor for every `Bind` request. If you need to maintain some form of state between strategy calls, you can do it via external state 
 object which you can pass around your strategy method calls


 ## Strategy Matching

 Azos provides flexible way of matching **the best** implementation for the requested strategy contract types. 
 You can certainly come up with custom ways implementing various [`IStrategyTrait`](Traits.cs).

 For example, you could put provider-specific strategy implementations in assemblies/namespaces/classes having their
 names correspond to the requested context (e.g. based on pattern, such as "FedexShipping" vs "USPSShipping" etc.)

 The system provides a built-in scoring pattern/matching system.

 The following unit test provides an example of implementing a custom geo-proximity based strategy pattern matching system:
 [StrategyPatternsTests.cs](/src/testing/Azos.Tests.Nub/Application/StrategyPatternsTests.cs)









 ---
Back to [Documentation Index](/src/documentation-index.md)

External resources:
- [Strategy Pattern (Wikipedia)](https://en.wikipedia.org/wiki/Strategy_pattern)
- [Dependency Injection (Wikipedia)](https://en.wikipedia.org/wiki/Dependency_injection)





