# Azos Logging

Back to [Documentation Index](/src/documentation-index.md)

Logging is necessary in any kind of application, therefore it is the first service booted by Application Chassis.
Logging is represented by [`ILog`](/src/Azos/Log/Intfs.cs) contract having chassis reference `IApplication.Log`.

The Azos.Log has the following traits:
- Designed for use in local and **distributed systems** (e.g. Sky)
  - Built for high-performance async-first processing
  - Built-in **per-sink SLA** which drives in-built EMA filter-guarded circuit breaker
  - Every message captures a **host name**, which is set to a local machine name or global host name in a Sky cluster
  - Exceptions are not necessarily serializable, therefore they can not be marshaled across cluster boundaries as-is. 
    [`WrappedException`](/src/Azos/Exceptions.cs#L284) type takes care of this by capturing custom exception information and making it suitable for marshaling and long term storage (i.e. archiving)
- Supports **log message archiving**:
  - Built-in BSON serialization suitable for binary logs and long-term storage (e.g. as is in MongoDb)
  - The system extracts `ArchiveDimensions` which may be used for OLAP warehousing
- Multi-channeling 
  - Applications may use different "planes" aka "channels" of logging for different purposes
  - Channels may be processed differently, e.g. a security channel may stash the log in a long-term archive, it is possible to create a separate **channel for member enrollment or other business-specific** process

## Log Message
Azos logging is async-first, therefore it is built around a [`Message`](/src/Azos/Log/Message.cs) sealed class. `Message` is BSON-serializable for long-term storage.

Messages header properties have the following cardinality:
1. Channel: string - messages are processed by channels, e.g. "Security", "Social" etc..
2. Topic - within a channel messages are split by topics. Topics are to be taken from non-localizable constants, e.g. "RPC", "financial" etc.. Topics represents logical subs-systems/areas.
3. From: string - designates where message emitted from within the topic, aka. 'ComponentName'
4. Source: int - tracepoint/line# within component

The following describes `Message` properties:

<table>
<tr><th>Property</th><th>Description</th></tr>

<tr>
  <td>Gdid <br><sup>GDID</sup></td>
  <td> Global distributed ID used by distributed log warehouses. The field is assigned by distributed warehouse implementations such as Sky Chronicle Logic. GDID.ZERO is used for local logging applications which do not use distributed ids.</td>
</tr>

<tr>
  <td>Guid <br><sup>Guid</sup></td>
  <td> Provides global unique identifier for this log message instance. GDID.ZERO is used for local logging applications which do not use distributed ids.</td>
</tr>

<tr>
  <td>RelatedTo <br><sup>Guid</sup></td>
  <td> Gets/Sets global unique identifier of a message that this message is related to. No referential integrity check is performed.</td>
</tr>

<tr>
  <td>App <br><sup>Atom</sup></td>
  <td> Identifies the emitting application by including it asset identifier, taken from App.AppId.</td>
</tr>

<tr>
  <td>Channel <br><sup>string</sup></td>
  <td> 
   Gets/Sets logical partition name for messages. This property is usually used in Archive for splitting destinations
  </td>
</tr>

<tr>
  <td>Type <br><sup>MessageType</sup></td>
  <td> Gets/Sets message type/severity, such as: Info/Warning/Error etc...</td>
</tr>

<tr>
  <td>Source <br><sup>int</sup></td>
  <td> Gets/Sets message source line number/tracepoint#, this is used in conjunction with From. 
    Tracepoints are logical block/line numbers which are iused if code changes often (e.g. Source: 100, 200, 300)</td>
</tr>

<tr>
  <td>UTCTimeStamp <br><sup>DateTime in UTC</sup></td>
  <td> Gets/Sets UTC timestamp when message was generated. The stamp is always in UTC. The log display screens, may offset the date
per display location - this is a UI concern. All log messages are archived/warehoused in UTC only </td>
</tr>

<tr>
  <td>Host <br><sup>string</sup></td>
  <td> 
     Gets/Sets host name that generated the message. On a local Azos application this defaults to Message.DefaultHostName static,
which may not be set in which case local computer name is used. In Sky applications this defaults to SKy distributed cluster host name e.g. 
'/world/us/east/nyc/web/lnxmedium001'
  </td>
</tr>

<tr>
  <td>From <br><sup>string</sup></td>
  <td> Gets/Sets logical component ID, such as: class name, method name, process instance, that generated the message.
    This field is used in the scope of Topic </td>
</tr>

<tr>
  <td>Topic <br><sup>string</sup></td>
  <td> 
   Gets/Sets a message topic/relation - the name of software concern within a big app, e.g. "Database", "Security" ...
   The `From` message property provides more detailed location under the specified `Topic`
 </td>
</tr>

<tr>
  <td>Text <br><sup>string</sup></td>
  <td> 
     Gets/Sets an unstructured message text, the emitting component name must be in From field, not in text.
     Note about logging errors: Use caught exception.ToMessageWithType() method, then attach the caught exception as Exception property for
structured logging
  </td>
</tr>

<tr>
  <td>Parameters <br><sup>string</sup></td>
  <td> 
   Gets/Sets a structured parameter bag, this may be used for additional debug info like source file name, additional context etc.
   The bag content is typically in JSON format
  </td>
</tr>

<tr>
  <td>Exception <br><sup>Exception</sup></td>
  <td> 
   Gets/Sets exception associated with log message.
   Set this property EVEN IF the name/text of exception is already included in Text as log sinks may elect to dump the whole stack trace
  </td>
</tr>

<tr>
  <td>ArchiveDimensions <br><sup>string</sup></td>
  <td> 
   Gets/Sets archive dimension content for later retrieval of messages by key, i.e. a user ID may be used.
   In most cases JSON or Laconic content is stored, the format depends on a concrete system. See `ArchiveDimensionMapper`
  </td>
</tr>


</table>





## Component Logging
When developing application components (such as business logic modules), `ApplicationComponent.WriteLog(...)` should be used 
instead of direct `App.Log.Write(msg)`. The method checks `ComponentEffectiveLogLevel` property and sets proper `Topic` and `From` prefixes 
per component:
```CSharp
private void logicBody(DomainObject data)
{
  //get message correlation Guid, then relate other log entries to it
  //the nameof(logicBody) is used as a suffix for the `From` property
  var thisCall = WriteLog(MessageType.Trace, nameof(logicBody), "Entering...");
  try
  {
    //do some business logic with passed-in data object
    //handle all exceptions so nothing leaks
  }
  catch(Exception error)
  {
    //should ANYTHING unexpected leak we log it
    WriteLog(MessageType.Critical, nameof(logicBody), "Unexpected leak: {0}".Args(error.ToMessageWithType()), error, relatedTo: thisCall);
    
    //instrument the failure
    MemberProcessingErrorEvent.Happened(App.Instrumentation, member.ID);
  }
  WriteLog(MessageType.Trace, nameof(logicBody), "...Exited", relatedTo: thisCall);
}
```

In the example above, if the containing app component has its `ComponentLogLevel` set above `Trace` then tracing messages are 
NOT going to get logged.

## Terminal Error Logging
When developing CLI and multi-threaded applications it is a good practice to **handle all of the error types** to ensure that leaking an unexpected
**exception does not crash your service daemon**. Log the catch-all errors and emit an instrumentation event - this is a better pattern than crashing a 
process due to an unhandled error thrown on one of the 100s of cluster hosts.

#### The general rule is:
 **all terminal points in the app must use catch-all handlers** and gracefully terminate the app if necessary or keep running.
A **terminal application point** is the one where an exception has no place to propagate further - program entry points and thread bodies.
Unhandled exceptions terminate the hosting OS process if exceptions are not handled at the terminal points.

The following pattern should be used for handling errors at CLI program terminal points:
```CSharp
class Program
{
  public static int Run(string[] args)
  {
    using(var app = new AzosApplication(args))
    {
      try
      {
          return run(app);
      }
      catch(Exception error)
      {
          App.Log.Write(new Message{
            Type = MessageType.CatastrophicError,
            Topic = nameof(Program),
            From = nameof(Run),
            Text = "Leaked: {0}".Args(error.ToMessageWithType()),
            Error = error
          });
         return -1;//environment error exit code
      }
    }
  }

  private int run(IApplication app)
  {
    if (app.CommandArgs["help"].Exists)
     .....
  }
}
```


The following pattern should be used for handling errors at `ApplicationComponent` terminal points (such as a thread body):
```CSharp
private void threadBody()
{
  while(Running)
  {
    try
    {
      doWorkCore();//not expected to crash
    }
    catch(Exception error)
    {
      UnexpectedErrorEvent.Emit(App, error);//instrumentation
      WriteLog(MessageType.Error, nameof(threadBody), "Leaked unexpected: {0}".Args(error.ToMessageWithType()), error);
      //we may want to wait here a bit to debounce the error
    }
  }
}
```

The `ApplicationComponent.WriteLog()` above will pre-pend the `From` field with the component class name so it looks something like
(just as an example): `FTPImportDaemon.threadBody`,
this way just by looking at `From` you can immediately know where the error came from. Another important aspect in the pattern above is the
passthrpough of exception instance into the log message. This is needed so that relevant sinks, such as `DebugSink` could have access to the 
exception instance for structured logging. **Notice the use of `ToMessageWithType()`** extension which shows exception type and text. In Azos, 
application **logic should throw custom-typed exceptions** - this **significantly simplifies debugging and support** and obviates the need in many
 cases to look at lengthy stack traces as exception type names are descriptive enough. 

Contrast:
```CSharp
 [InvalidOperationException] Index is out of bounds at "c:\project\MyProject\src\MyCore\....
.... 70 lines of stack trace ...
   
  -  vs  -

 [MemberRateCalculationError] Could not calculate billing rate for member `R-324348`. 
  DoCalculate() leaked: [InvalidOperationException]Index is out of bounds at... 
... 70 lines of stack trace ...
```
in the second case, just by looking at exception type and text you can pretty much pinpoint the error location.



## Basic Configuration
Logging is typically configured by the app chassis at its boot. The following is a typical simple configuration with one catch-all sink and one debug sink that logs errors only:
```CSharp
app
{
  name="myapp" // name of the application
  
  paths//set all app paths here (this is not required, but recommended)
  {
    disk-root=$(~MY_APP_HOME) //disk root - take it from MY_APP_HOME env variable
    log-path=$($disk-root)logs
  }

  log //this is app root logging component
  {
    name="applog"
    write-interval-ms=500//flush queues once 0.5 sec

    sink//logs everything
    {
      name="default" 
      type="Azos.Log.Sinks.CSVFileSink, Azos" 
      path=$(/paths/$log-path)
      file-name="$(/$name)-{0:yyyyMMdd}-$($name).log.csv"//myapp-20181220-default.log.csv
    }

    sink//log debug details
    {
      name="debug" 
      min-level=Error//process errors and more severe conditions
      type="Azos.Log.Sinks.DebugSink, Azos" 
      path==$(/paths/$log-path)
      file-name="$(/$name)-{0:yyMMdd}-$($name).log"//myapp-20181220-debug.log
    }
  }
}
```

## Log Sinks
Log component can work with any custom `Sink`-derivative.

**Note:** sinks are synchronous, the [`LogDaemon`](/src/Azos/Log/LogDaemon.cs) is asynchronous. 
There is a [`LogDaemonSink`](/src/Azos/Log/Sinks/LogDaemonSink.cs) which is based on asynchronous 
inner daemon. The following sinks are supplied right out of the box:

- **CSVFileSink** - dumps messages into local files in the parsable CSV file format
- **CompositeSink** - creates a wrapper around multiple child sinks, used to set filtering and routing rules for sink sub-group
- **ConsoleSink** - writes log messages into STDOUT/Console, with optional coloring
- **DebugSink** - textual file format with detailed exception/stack tracing 
- **FloodSink** - pass-through composite which throttles the number of messages written per set limit (ideal for preventing EMail flooding etc.)
- **JSONSink** - writes messages into text file in JSON format 
- **LogDaemonSink** - an asynchronous sink based on inner logger daemon instance
- **MemoryBufferSink** - buffers messages in a circular buffer with ability to query the buffered content
- **NullSink** - does nothing
- **SMTPSink** - sends messages using SMTP protocol/server connection
- **SyslogSink** - writes into *NIX syslog daemon using UDP cast
- **MongoDBSink** - writes messages into MongoDB collection (NoSQL)
- **MsSqlDBSink** - writes messages into Microsoft SQL RDBMS table
- **MySqlDBSink** - writes messages into ORACLE MySQL RDBMS table
- **SkySink** - sends log messages to the LogReceiver microservice in the Sky cluster
- **SkyZoneSink** - routes log messages to parent Sky cluster zone governor

---
Back to [Documentation Index](/src/documentation-index.md)