# Fabric 

Fabric is a **distributed cooperative multitasking process** framework.

Fabric runs logical processes called **Fibers** on a distributed node cluster, which
can dynamically scale up or down.
Fibers are akin to OS's processes - they represent some unit of logical execution, such as 
system or business flows (processes) which can run for hours, days, years or even indefinitely.
Just like an OS process, a `Fiber` has start parameters, private execution state (aka `FiberMemory`)
exit code, and exit result objects. 


> Fibers execute code in two ways: 
> a. periodically execute their next scheduled time slices - upon execution, each slice returns when 
     the next execution should take place (which can be ASAP or in a very distant future/never)
> b. react to signals - a form of close to real-time RPC returning a signal response


Fibers implement **cooperative multitasking paradigm** - they execute time slices, yielding control back
to the fiber runtime system ASAP (by design). These running time slices are also called "steps" of execution. 
The Fabric system **saves fiber's transitive/private state** (called `FiberMemory`) into persisted storage upon 
each slice execution.

> Fabric system is not designed for real-time (e.g. media stream) data processing, but instead is geared
> towards long-running batch/supervisor processes which control eventual execution of complex business flows

## Use Cases
The Fabric was built with the following use cases in mind:

1. Long-running flows of logical execution keyed on business aggregate roots (e.g. customers). Very useful for 
   business applications where the steps of the flow may not depend only on sequence, but on non-deterministic user 
   actions which can be in future, e.g. "customer contract signed"
2. Auditor/Watchdog process pattern - a long-running sidecar process which ensures consistency of critical records, 
   such as financial books, event logs, legal records, etc... creating compensating transactions/adjustments 
   when needed (e.g. Saga pattern)
3. Actor model - actor reacting to signals, supervisor processes ensuring actor valid state
4. System agents e.g. log archiving/rotation/sweep
5. Activity orchestration - a global supervisor fiber orchestrates others
6. Activity choreography - a fiber launches another one/ones
7. Completion slot/port - a fiber instance is used as a completion piece of state where other 
   processes (not necessarily fibers) accumulate distributed computation result, e.g. map-reduce long scan




   




## Fabric Components / Topology

The fabric system consists of the following component tiers
### Processors
Processors perform 2 major tasks: 
1. Execution of scheduled fiber slices
   1. Fetch pending scheduled `FiberMemory` instances from storage, placing an exclusive fiber lock
   2. Prepare fiber for execution: create an instance of `Fiber`-derived class,
      perform DI and other runtime parameter and state 
      fixup
   3. Execute fibers by invoking their time slice methods, handle exceptions
   4. Save fiber state back into the persisted store, releasing the lock
2. React to fiber signals
   1. Trying to obtain an exclusive fiber lock of `FiberMemory` instance
   2. Prepare fiber for execution: create an instance of `Fiber`-derived class,
      perform DI and other runtime parameter and state 
      fixup
   3. Execute fiber signal handler by invoking the signal handling method on a fiber instance, handle exceptions
   4. Save fiber state back into the persisted store, releasing the lock

### Store
The fiber store is a distributed sharded store of fiber state - akin to OS's RAM, this is where the process FSM state is stored.

As there is no processor:store shard affinity, the Fabric processors take EXCLUSIVE lock-per fiber instance.
The lock are implemented in an efficient in-store way (e.g. SELECT FOR UPDATE) which insures a single logical thread/process
execution at any given time.




## Examples

This example