# TODOs - Asynchronous Tasks/Functors and Queues

**Todos** - are a form of an asynchronous one-way commands. They are somewhat similar to .NET Task class (returning void),
only running in a distributed environment. Todo instances do not get executed at the time of their creation and do not
return any result, instead Todos get dispatched into `TodoQueue` via a corresponding network call dispatched via 
`HostSets`. 

Todo class is a lightweight model for sending data. Todos use tightly optimized Arow serializer for marshalling between
processes via highly optimized Glue bindings. Todos derive from `AmorphousTypedDoc` - a document for data having 
persisted state decorated with `[Field]` attribute and support version changes. See Azos documentation for `Azos.Data` namespace.

The Todo class declaration: 

```CSharp
/// Represents a unit of abstract work that is dispatched 
/// to a remote worker in an asynchronous fashion. Todos are 
/// essentially a form of a queueable asynchronous oneway 
/// command object (Execute() does not return business object).
/// Todos are dequeued in the order of submission and 
/// SysStartDate constraint, processed sequentially or inparallel 
/// depending on a SysParallelKey
[Serializable]
public abstract class Todo : AmorphousTypedDoc, IDistributedStableHashProvider
{
  // Factory method that creates new Todos assigning them new GDID
  public static TTodo NewTodo<TTodo>() where TTodo : Todo, new () { ... }

  protected Todo() { }

  // Globallyunique ID of the TODO
  public GDID SysID { get; }

  // When was created
  public DateTime SysCreateTimeStampUTC { get; }

  // Provides the sharding key which is used for dispatching items into HostSets
  public string SysShardingKey { get; set; }

  // Provides the key which is used for parallel processing:
  // items with the same key get executed sequentially
  public string SysParallelKey { get; set;}

  // Provides relative processing priority of processing
  public int SysPriority { get; set; }

  // When set, tells the system when (UTC) should the item be 
  // considered for processing
  public DateTime SysStartDate { get; set; }

  // Provides current state machine execution state
  public ExecuteState SysState { get; internal set; }

  // Provides current machine execution retry state
  public int SysTries { get; internal set; }

  // Executes the todo. Override to perform actual logic.
  // You have to handle all exceptions, otherwise the leaked 
  // exception will complete the todo with error. Return the 
  // result that describes whether the item completed or 
  // should be reexecuted again.
  // Keep in mind: Todos are not designed to execute longrunning
  // (tens+ of seconds) processes, launch other async workers instead
  protected internal abstract ExecuteState Execute(ITodoHost host, DateTime utcBatchNow);

  // Invoked to determine when the next reexecution takes place 
  // after an error. Throw exception if your business case has
  // exhausted all allowed retries as judged by SysTries.
  // Return 1 to indicate the immediate execution without consideration 
  // of SysTries (default)
  protected internal virtual int RetryAfterErrorInMs(DateTime utcBatchNow) {...}
}
```

The following example declares a simple "lambda" function expressed as an instance of Todo with the fields containing 
persistent state decorated with [Field] attribute. The Execute() method gets invoked on the queue processing server, 
the Complete flag signals the runtime to discard the instance after successful execution: 

```CSharp
[TodoQueue("email", "A0176D65B43C4D349DBAEDACB281709F")]
public class EmailTodo : Todo
{
  public EmailTodo() { }

  [Field] public string Email   { get; set;}
  [Field] public string Subject { get; set;}
  [Field] public string Text    { get; set;}

  protected internal override ExecuteState Execute(ITodoHost host, 
                                                   DateTime utcBatchNow)
  {
    MyApp.EMailController.SendSimple(EMail, Subject, Text);
    return ExecuteState.Complete;
  }
}

...
    //Dispatch example
    var todo = Todo.NewTodo<EmailTodo>();
    todo.EMail = "test@gmail.com";
    todo.Subject = "Hello";
    todo.Text = "This message is from todo";
...
    queue.Enqueue(todo);


```

## Programming Models

Todos support various programming models. The **"maximum"** programming model is a **state-full functor** - that is - 
**code with data state**. This is a very flexible approach because state is an option, but **not required** so it allows
 to model different computational patterns, including the stateless one-way messaging.

The following models can be used with Todos:

* A classic stateless messaging system - immutable one-way calls, no logic in Todo (logic is in the separate Actor/receiver)
* 100% stateful analogue of .NET task - with ability to use FSM with persistence (a message-initiated Actor)
* One way lambda functions a-la **"serverless/cloud functions"**

Let’s consider the models described above. 

**In a classical stateless immutable message style of programming**, a Todo instance is just a data vector with no logic 
attached (blank Execute method). Todos are sent into queues and processed only once and then discarded. 
They can also be saved into an event store for later replay. This approach is used in **Actor-model** systems like 
**Erlang** or **Akka**.

**The stateful design** is the one where the instance’s state may be changed by the Execute() or other means and
 **the same Todo instance may be re-enqueued/rescheduled** in future or discarded. The state of todo may be expressed
 via an imperative FSM with full persistence (done by the Sky) of the transitive states in the durable storage.
 Effectively, this turns **Todo queue into Todo pool/bag** - as **objects may re-enter the pool**. The Todos are still 
ordered by scheduled execution time. Of course this is the least efficient way of working with Todos performance-wise 
as the state needs to be saved after changes, but it is sometimes the most practical and  convenient way for writing 
business logic.

**One way lambda functions** are asynchronous Execute() invocations that can create other Todos or just complete. This 
approach works like a popular **"serverless"** paradigm (e.g. AWS Lambda). 

## HostsSets and Dispatching

The Todos get dispatched into HostSets (described in more details further), the distribution within HostSet is done 
based on a Todo.SysShardingKey property: 

`public string SysShardingKey { get; set; }`

The queue sequence is maintained based on the Todo.SysStartDate:
`public DateTime SysStartDate { get; set; }`

The HostSet members execute Todos sequentially, in-parallel, or parallel by-key:`public DateTime SysParallelKey { get; set; }`

as configured on a queue level execute mode: 
```CSharp
 ...
 queue { name='email' batchsize=1024 mode = Parallel }
 queue-store
 {
   type='Azos.Sky.Workers.Server.Queue.MongoTodoQueueStore, Azos.Sky.MongoDb'
   mongo='mongo{server="localhost:27017" db="queue-test"}'
   fetch-by=4000
 }
 ...

```
<img src="/doc/img/todo-queues.png">

Todos are descendants of `TypedDoc` and allow for state stored in [Field]-decorated properties as required by domain 
logic. In case of stateful Todos the data is stored in a durable store. The execution logic may be plain imperative code
 or use Finite State Machine (FSM) with its transient states persisted. 

In case of FSM, the current execution state is stored in `SysState` field:

```CSharp 
  public ExecuteState SysState { get; internal set; }
```

The execution takes place in the abstract Execute() method:

```CSharp
 protected abstract ExecuteState Execute(ITodoHost host, DateTime utcBatchNow);
```

which returns the next status of state machine. 
The Todo will be called until it transitions into (Complete) state. 

## State Machine Example
An example of a complex state machine that handles complex customer Order state, and transitions it through a workflow
 as defined by the corresponding **FSM state transitions**. Notice a complex business object `Order` - is 
**supplied as a field of the todo** instance. 

```CSharp
 [TodoQueue(SysConsts.TQ_SHOP, "76708068CE024B1A9FE6A9054AA33CB1")]
 public class TakeOrderTodo : Todo
 {
  static readonly ExecuteState State_AssignPart = ExecuteState.Initial;
  static readonly ExecuteState State_StoreInKDB = new ExecuteState(1);
  static readonly ExecuteState State_StampMLinks = new ExecuteState(2);
  static readonly ExecuteState State_StampPays = new ExecuteState(3);
  static readonly ExecuteState State_Register = new ExecuteState(4);
  static readonly ExecuteState State_Insert = new ExecuteState(5);
  static readonly ExecuteState State_SendEmail = new ExecuteState(6);
  static readonly ExecuteState State_EnqueueAutoApprove = new ExecuteState(7);
  static readonly ExecuteState State_EnqueueForceVendorActivityReport = new ExecuteState;

  [Field] public Data.Rows.ORDER.OrderRow Order { get; set; }
  [Field] public UserInfo UserInfo { get; set; }
  [Field] public int AutoApproveHrs { get; set; }

  protected override ExecuteState Execute(ITodoHost host, DateTime utcBatchNow)
  {
             if (SysState == State_AssignPart)
             {
                GDID gPart = GDID.Zero;
                if (!Order.G_Part.HasValue)
                {
                    try
                    {
                        gPart = getOrderPatition(...);
                        if (gPart.IsZero) return ExecuteState.Reexecute;
                    }
                    catch (Exception error)
                    {
                        ... log error
                        return m_IsModified ?
                            ExecuteState.ReexecuteUpdatedAfterError :
                            ExecuteState.ReexecuteAfterError;
                    }
                }
                ...
                return State_StoreInKDB;
            }
            else if (SysState == State_StoreInKDB)
            {
                MyiApp.Data.Catalog.StoreOrderID(Order.ID);
                return State_StampMLinks;
            }
            else if (SysState == State_StampMLinks)
            {
                ...stampLinks();
                return State_StampPays;
            }
            else if (SysState == State_Insert)
            {
                ...insert OrderInDb();
                return State_SendEmail;
            }
            else if (SysState == State_SendEmail)
            {
                ...sendCustomerOrderEmail()
                return AutoApproveHrs < 0 ?
                    ExecuteState.Complete : 
                    State_EnqueueAutoApprove;
            }
            else if (SysState == State_EnqueueAutoApprove)
            {
                // Enqueue auto approve of the order in future
                try
                {
                    var todo = NewTodo<ApproveOrderTodo>();
                    todo.G_Vendor = Order.G_Vendor;
                    todo.OrderID = Order.GDID;
                    todo.Operator = "sys";
                    todo.Note = "AUTO APPROVE";
                    todo.SysStartDate = Order.Date.AddHours(AutoApproveHrs);
                    Todoer.EnqueueInOrder(todo);
                }
                catch (Exception error)
                {
                    ...log error
                    return m_IsModified ?
                        ExecuteState.ReexecuteUpdatedAfterError :
                        ExecuteState.ReexecuteAfterError;
                }
                return ExecuteState.Complete;
            }
        }

  //Example of throttling-down the process upon subsequent processing errors
  protected override int RetryAfterErrorInMs(DateTime utcBatchNow)
  {
    const int MAX_ENQUEUE_AUTO_APPROVE_RETRIES = 50;
    if (SysState == State_EnqueueAutoApprove &&
        SysTries > MAX_ENQUEUE_AUTO_APPROVE_RETRIES)
      ...order can not be processed... send email or send to reject queue for later
      consideration...
      //SysTries increases with every iteration
      var ms = ExternalRandomGenerator.Instance.NextScaledRandomInteger(1000, 3000);
      return Math.Min(4 * ms + 5 * 60000, ms * SysTries );
  }
}
```

In the example from above, the Order object is supplied from the customer (e.g. from a front end application).
 The system starts to execute the TakeOrderTodo by calling its Execute() method for every state until the Complete
 state is returned. If a failure happens, the Todo is re-scheduled for continued execution at the point of the error -
 this loops the instance through the persistent storage. The RetryAfterErrorInMs() method allows to forestall the 
domino effect of accumulating errors, the random generator ensures that no time frames are equal - it is a good practice
 to add randomness in distributed systems to prevent "all at once" or resonance (periodic amplification) effects. 


