 # Distributed Locking/Coordination 
 Distributed locking feature facilitates global synchronization at any system topography level with just a few 
lines of code. It is also used for coordination/scheduling of running processes. The data contained in the
 locking servers is considered to be ephemeral, there is no need to persist it, consequently
 **Sky OS locking is a pure in-memory solution** without making any on-disk copies or cross-device state synchronization. 

 ## Probability-based Speculative Synchronization

 **The approach is purely speculative, however it works well in practice.**

 The failures are handled based on a **reliability contract/expectation** which is stipulated by every transaction.
 The approach is based on an efficient one-phase client-server protocol that **statistically ensures** 
(*but does not guarantee*) that **client caller can rely on the server state**. It achieves high throughput by 
 **completely eliminating state machine sync inter-node communication** - so typical in systems that use Raft or Paxos. 
Sky locking system ensures that the client transaction/call succeeds only when the
 **minimum probable server state/data currency claim is satisfied**.

 The **reliability contract**(expectation) is expressed as the minimum level of TRUST that the server has to achieve 
for transaction to succeed:  

* We measure the server system uptime (in units of time, e.g. seconds)

* We measure the **current TRUST level on the server** host based on the request throughput:
  * Measure the call rate "norm" as obtained on the particular server box. 
  * Any decline in the call rate indicates a possible networking issue which may have precluded other callers from publishing their state to the server
  * Therefore, we **assume probabilistically that the server data is not really up-to-date**

* Every time a client makes a call to the server, it (the client) supplies the minimum required reliability guarantees: uptime (i.e in seconds) and minimum require TRUST level (i.e. 0..1.0). Shall the server not qualify per the requested guarantees, the client transaction gets denied as the server is not in the consistent state. 

### Call Norm - QOS

Call norm calculation works like this. We measure the call rate in ops/sec, then feed the number into 
EMA(Exponential moving average) filter. The filter renders the "average" insensible to suddent "spikes" in traffic - 
the EMA coefficient is attuned to "not feel" the traffic spikes lasting for split seconds. We now apply a decay function,
 which lowers the norm with time, thus when the server becomes idle, the "norm expectation" diminishes. The "TRUST" level 
is a ratio of  `current_load / NORM` - which is basically a **QOS (Quality of Service)** indicator. 

Suppose a server handles 20 req/sec - if this rate sustains for a few seconds, the NORM jumps to around 20/sec. Now,
 something happens to the network/switch and the calls drop to 2 req/sec - the NORM is going to hang around 20 ops/sec
 for some time until it decays into the lower range, at that time the TRUST is going to be low because of the abrupt drop
 in traffic. As stated above, **this is purely speculative way of** assessing the server "accuracy", for example - it declines
 transactions after "bouts" of activity caused by other services - but this is well expected and eventually the system evens-out.
 The chief benefit is the absence of complexity and latency - **this solution avoids making any extra calls completely**!

> *Many high-scalability architectures rely on the Paxos, Vector Clocks, Raft, Byzantine Fault Tolerance and other complex 
> algorithms of achieving the consensus in a distributed systems. These methods are usually very complex to implement/test
>  properly and they cause extra network traffic as required by multi-phase protocols. Our approach obviates the need to execute
>  multi-phase (multiple) networking calls as the currency/uptime of the system may be asserted with a single call with 
> acceptable degree of probability...*

## Locking Data Structures

The locking/coordination structures are based on the **named tables** *(called "variables")* with primary keys. The most typical
 locking operation is an attempt to **insert a key in the table** and **get a key violation** that would indicate that other 
process has already placed the record for that particular key. The locking server executes transactions in the caller's 
**session context which has an expiration lifespan**. Shall a caller never roll-back the transactions/kill session, the 
**server would delete ALL variable entries from its memory upon expiration**. On a typical server machine with 8 Gb of RAM and 
4 cores the locking server can support hundred of thousands of lock var entries executing over 200,000+ transactions a second.

In practice these data structures are used to coordinate co-operating tasks such as map-reduce, full scans and the like where a 
swarm of hosts exchange data describing "chunks" of work that they perform. In reality this leads to generation of 10s-100s of 
variable entries at most, which makes this solution **VERY efficient for real-time multi-worker coordination**. See [Distributed Coordination](../Coordination)

<img src="/doc/img/locking-ns.svg">

All **transactions are executed in a 100% serializable mode** *(one after another)* within a namespace. A server supports 
multiple namespaces, in which it creates/mutates variables.

 A **transaction is a set of operations** *(organized as expression tree)* submitted to the lock server for execution. 
**Either all** operations succeed **or none** at all. The transaction operations are broadly classified as: `set`, `delete`,
 `read` and `logical` operators. 

## Example

 Example of complex locking in a medical data acquisition system:

 ```CSharp
  var mdsEnter = new LockTransaction(
      "MDS Entry for Zak Pak@'Cherry Grove'", // description  
      "Clinical", // namespace
      0, 0.5d,    // trust level
      LockOp.Assert( LockOp.Not(LockOp.Exists("Month-End", "CGROVE-35"))),
      LockOp.Assert( LockOp.Not(LockOp.Exists("MDS-Review", "CGROVE-35", "R-45899"))),
      LockOp.Assert( LockOp.SetVar("MDS-Entry", "CGROVE-35", "R-45899", allowDuplicates: true))
  );

  var mdsReview = new LockTransaction(
      "MDS Review for Zak Pak@'Cherry Grove'", 
      "Clinical",
      0, 0.5d,
      LockOp.Assert( LockOp.Not(LockOp.Exists("Month-End", "CGROVE-35"))),
      LockOp.Assert( LockOp.Not(LockOp.Exists("MDS-Entry", "CGROVE-35", "R-45899"))),
      LockOp.Assert( LockOp.SetVar("MDS-Review", "CGROVE-35", "R-45899", allowDuplicates: false))
  );

  var mdsReviewUnlock = new LockTransaction(
      "MDS Review for Zak Pak@'Cherry Grove' is done", 
      "Clinical", 
      0, 0.5d,
      LockOp.AnywayContinueAfter(LockOp.DeleteVar("MDS-Review", "CGROVE-35", "R-45899"))
  );
```

The above example is modeled in accordance with the following business case: 

* There are medical facilities. `"CGROVE-35"` is used in the example
* Medical facilities have multiple patients, `"R-45899"` is used in the example
* MDS is a set of data filled for every patient.
* Multiple MDS Entry transactions can take place for the same patient at the same time
* Only one MDS Review transaction can take place for the same patient at the time
* Entry and Review are mutually exclusive modes
* None of the MDS activity can take place IF the Financial `MONTH-END` procedure takes place in the facility (regardless of patient) 

The `mdsEnter` lock is formed according to the rules, pay attention to `"SETVAR"` `allowDuplicates: true` for enter. 
The `mdsReview` `allowDuplicates: false` - as only one operator can take the lock. 

Both statements assert pre-conditions that other conflicting statuses are not set in the system. 

```CSharp
  var result = server.ExecuteLockTransaction(user1, mdsEnter);
  Assert.AreEqual(LockStatus.TransactionOK, result.Status);

  result = server.ExecuteLockTransaction(user2, mdsEnter);
  Assert.AreEqual(LockStatus.TransactionOK, result.Status);

  result = server.ExecuteLockTransaction(user3, mdsReview); 
  //Can't start review because someone else is entering
  Assert.AreEqual(LockStatus.TransactionError, result.Status);
  Assert.AreEqual("1:/AssertOp/", result.FailedStatement);
```