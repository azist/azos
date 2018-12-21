# Azos Logging

Back to [Documentation Index](/src/documentation-index.md)

Logging is necessary in any kind of application, therefore it is the first service booted by Application Chassis.
Logging is represented by [`ILog`](/src/Azos/Log/Intfs.cs) contract and mounted on `IApplication.Log` property.

The Azos.Log has the following traits:
- Designed for use in local and **distributed systems** (e.g. Sky)
  - Built for high-performance async-first processing
  - Built-in per-sink SLA which drives in-built EMA filter-guarded circuit breaker
  - Every message captures a **host name**, which is set to a local machine name or global host name in a Sky cluster
  - Exceptions are not necessarily serializable, therefore they can not be marshaled across cluster boundaries as-is. 
    [`WrappedException`](/src/Azos/Exceptions.cs#L284) type takes care of this by capturing custom exception information and making it suitable for marshaling and long term storage (i.e. archiving)
- Supports message archiving
  - Built-in BSON serialization suitable for binary logs and long-term storage (e.g. as is in MongoDb)
  - The system extracts `ArchiveDimensions` which may be used for OLAP warehousing
- Multi-channeling 
  - Applications may use different "planes" aka "channels" of logging for different purposes
  - Channels may be processed differently, e.g. a security channel may stash the log in a long-term archive, it is possible to create a separate **channel for member enrollment or other business-specific** process

## Component Logging

While developing application components (such as business logic modules), `ApplicationComponent.WriteLog(...)` should be used 
instead of direct `App.Log` access. The method checks `ComponentEffectiveLogLevel` property and set proper `Topic` and `From` prefixes 
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
There is a [`LogDaemonSink`](/src/Aozs/Log/Sinks/LogDaemonSink.cs) which is based on asynchronous 
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