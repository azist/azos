# Distributed Cluster Process Execution Model

Azos Sky provides mechanisms for **scheduling**, **dispatching** and **control of executable primitives** on different levels of abstraction.
These mechanisms are not required to write distributed apps, however they provide a broad range of functionality required by the majority of
application use-cases. 

For example: 
- enqueue event notifications/messages in event-based systems
- send customer emails notifications correlating on topic
- financial books auditing - scan transaction shards vs books for discrepancies - generate adjustements
- executing MapReduce or in-memory summary jobs
- sending social messages/chat 

all of these tasks can be easily coded with the building blocks described below. 

Sky provides the following mechanisms each of which is described in the linked detail pages: 

* [**Todo**](Todo.cs) - an execution primitive akin to ["cloud function"](todos.md), a part of a larger task, provides partitioning of a larger task, parallelization and load balancing between cluster nodes. [More details on Todos...](todos.md)

* [**Process**](Process.cs) - a global context of the larger task execution, establishes context for multiple instances of asynchronous Todos, captures the final result of multiple [`Todo`](Todo.cs)s; reacts to [`Signals`](Signal.cs)[More details on processes...](processes.md)

* **HostSet** - a named set of hosts participating in some business activity. For example, hostset of [Todo Queue](todos.md) processors

* **Dynamic Host** - a prototype of a host which can get spawned by the Azos Sky automatically (i.e. HostSet load balancer) or manually on a pluggable IaaS provider - an instance of CloudSystem 