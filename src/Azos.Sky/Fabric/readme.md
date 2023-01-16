# Fabric 

The Fabric is a **distributed cooperative multitasking process** framework.

Fabric runs logical processes called **Fibers** on a distributed node cluster, which
can dynamically scale up or down.
Fibers are akin to OS's processes - they represent some unit of logical execution, such as 
system or business flows (processes) which can run for hours, days, years or even indefinitely.

Fabric fibers have the following traits/properties:
1. Conceptually similar to OS process with a single main thread
2. Fibers are FSM - Finite State Machines with well define terminal status: Created|Started|Paused|Suspended|Finished|Crashed|Aborted
2. Have start parameter object (like command line args in os)
3. **Mutable Private State** - aka `FiberMemory` like variables in code/methods which is PRESERVEDbetween serves/calls/time slices
5. Fiber memory (state) is version upgradeable allowing for transparent data change on as-needed basis 
4. Provides exit code: int (just like an OS process)
6. Optionally provides a result object which can be queried
8. Execute periodically without any external actions - called fiber time slices or "steps"
9. A fiber slice should complete ASAP within seconds at most and yield control back - this is cooperative multitasking
9. Each timeslice returns the next step and when (in what interval) it should be executed
10. Fibers can schedule to run very infrequently (e.g. once a month or once a year) compared to OS processes as their state is persisted
11. Fibers can optionally react to signals - a way of RPC (remote procedure call), signals are processed synchronously, without waiting 
    for the next time slice
12. There is **no need to lock (synchronize) state** in a fiber as the Fabric guarantees that within its origin (home cluster partition) only
    one processor executes any given fiber at any given time 

Fabric implements **cooperative multitasking paradigm** using fibers - they execute time slices, deterministically 
yielding control back to the Fabric fiber runtime system ASAP (by design). These running time slices are also called 
"steps" of execution. The Fabric system **saves fiber's transitive/private state** (called `FiberMemory`) into persisted 
storage upon each slice execution. 

> This is somewhat similar to a concept of co-routines or `yield return` or `await` in C#, creating a `cutpoint`
> in code. The difference is that Fabric fibers can execute in clusters of processor nodes and they survive
> process restarts, their memory is version-upgradeable as the system design changes (e.g. add/remove data fields).

The system uses various techniques to optimize and ensure the timely (as scheduled) invocation of fiber slice methods,
however the following should be taken into consideration
1. Fabric by design does NOT guarantee execution of fibers just on time, hence it is NOT designed 
   for real-time/time sensitive processing
2. Fabric scheduler executes "the most due" fibers first, effectively creating a queue
3. If the system gets inundated with too many pending fibers while having too few processor nodes the latency would increase
4. Fabric provides per-fiber average and instant latency measurements
5. Since Fabric relies on cooperative multitasking, a rogue fiber implementation may not yield control back to processor
   for a significant amount of time (10s of seconds). This is bad design as it creates coarsely-grained slices which impede
   performance. The Fabric tries to time-out such slice tasks and may even abort a fiber for policy violation

> Important: Fabric system is **not designed for real-time** (e.g. media stream) data processing, but instead is geared
> towards long-running batch/supervisor processes which control eventual execution of complex business flows.
> Depending on Fabric cluster load, you need to expect multi-second latency deviation in fiber scheduled execution on 
> average

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