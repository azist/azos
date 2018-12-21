# Azos Instrumentation and Telemetry

Back to [Documentation Index](/src/documentation-index.md)

Instrumentation is a built-in feature of Application Chassis and does not require any 3rd party libs. It is somewhat
 similar to logging in a sense that it processes data messages and sends them to sink for storage/processing/visualization. 
However there are a few key differences, one being that instrumentation performs map:reduce of captured data before writing them to
instrumentation provider.

Instrumentation provides a uniform way of emitting instances of [`Datum`](/src/Azos/Instrumentation/Datum.cs) (singular of Data). There are two types of data messages processed by instrumentation stack:
- **Events** - events just happen, their primary data is their appearance at a certain time. An example would be `ProcessCrashEvent`
- **Gauges** - gauges are events with measurements (usually scalar). Example: `VoltageGauge` - measures AC voltage (measured in volts) at a certain time. A gauge measurement is an event of capturing a certain value

**You instrument your code by creating custom Datum-based classes** deriving from `Gauge` or `Event` classes, directly or indirectly. 
Make a measurement by allocating a new instance of the appropriate class and then call `App.Instrumentation.Record( datum )` method which routes 
your measurements down the processing stack.


## Instrumentation Contract

[`IInstrumentation`](/src/Azos/Instrumentation/Intfs.cs) provides the `Record(Datum)` method which writes measurement into the stack, it is synchronous because instrumentation stack is
 asynchronous by design and this method never blocks for long (expected to return in the microsecond range):

```CSharp
/// <summary>
/// Stipulates instrumentation contract
/// </summary>
public interface IInstrumentation : IApplicationComponent, ILocalizedTimeProvider
{
  // Indicates whether instrumentation is enabled
   bool Enabled { get;}

  // Records instrumentation datum
  void Record(Datum datum);
  ...
}
```

`Datum` class represents either a single measurement event or an aggregate, in which case Count &gt; 0.
The `InstrumentationDaemon` perform map:reduce of data in the background. This is where instrumentation stack differs from logging stack drastically.
Instrumentation performs mapping of measurements by their Datum-derived type, then it reduces
the data by calculating the aggregate measurement on a source data.

## Example Map:Reduce of Instrumentation Data

Consider the following example:
- Imagine an application that instruments an electrical power plant
- The Following data is captured, notice the use of Datum.Source for detalization of measurements by generator/plant: 
  - `GeneratorRPMGauge(source)`: measures revolutions per minute per measurement source
  - `GeneratorVoltageGauge(source)`: measures generated voltage in volts per measurement source
- The data is measured by various devices and captured by various threads at different times
- The instrumentation stack performs a mapreduce once every specified milliseconds interval
  - It groups all data by their type **and then** by their source
  - It creates a single new Aggregate datum out of grouped result obtained above per every grouping
  - It writes the resulting aggregated result into the sink (e.g. database or archive)

...therefore, we can control the level of detail by specifying the appropriate `source`. In the example above, we 
should probably measure `GeneratorRPMGauge(per generatorID)` and measure `GeneratorVoltageGauge(per generatorID)` - 
this way we will be able to track those parameters per every individual generator and cross-facility 
(for the null source).

You can override the following virtual method to control the level of instrumentation detail:
```CSharp
/// <summary>
/// Override to set a new source value which is less-specific than existing source.
/// ReductionLevel specifies how much detail should be lost. The function is idempotent, that is - calling more than once with the same arg does not
/// change the state of the object.
/// The default implementation removes all source details (unspecified source) when reductionLevel less than zero.
/// Example:
///  TotalBytesSent("mpx://45.12.123.19:7823 -> MySystem.Contracts.IDoSomething.SomeMethod1()")
///  ReduceSourceDetail(0) -> yields original string
///  ReduceSourceDetail(1) - > "MySystem.Contracts.IDoSomething.SomeMethod1()"
///  ReduceSourceDetail(2) - > "MySystem.Contracts.IDoSomething"
///  ReduceSourceDetail(3) - > ""
/// </summary>
public virtual void ReduceSourceDetail(int reductionLevel);
```

---
Back to [Documentation Index](/src/documentation-index.md)