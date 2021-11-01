# Azos Sky Chronicle

## Description:

The Azos Sky Chronicle provides simple, yet extremely powerful, API and web UI tooling on top of 
[Azos Logging](/src/Azos/Log). Designed for single instance hosts
or distributed cross-sharded cloud deployments. The Azos Sky Chronicle is engineered to typically be hosted
along side of any current application deployment or as a stand-alone deployment. Typically Sky Chronicle services are
configured to make use of an Azos appliance-mode embedded MongoDB instance as the primary backing store 
(See [BundledMongoDb.cs](/src/providers/Azos.MongoDb/BundledMongoDb.cs)).


![Log View](/doc/img/log-view.PNG)

![Log View](/doc/img/log-view-2.PNG)

======================================

# Chronicle (Logging) Domain Models:

## Filter Object Schema:

[LogChronicleFilter](/src/Azos.Sky/Chronicle/LogChronicleFilter.cs)

## MessageType Enum Schema:

[Enums.cs](/src/Azos/Log/Enums.cs)

> Debug, DebugA, DebugB, DebugC, DebugD, DebugError, DebugSQL, DebugGlue, DebugZ, 
Trace, TraceA, TraceB, TraceC, TraceD, TraceErrors, TraceSQL, TraceGlue, TraceZ, PerformanceInstrumentation,
Info, InfoA, InfoB, InfoC, InfoD, InfoZ, Aggregate, SecurityAudit, Notice,
Warning, WarningExpectation, Error, Critical, CriticalAlert, Emergency, CatastrophicError

## Log Message Schema:

[Log Batch](/src/Azos.Sky/Chronicle/LogBatch.cs)

[Log Message](/src/Azos/Log/Message.cs)

======================================

# Chronicle Log Message Model Details:

Azos logging is async-first, therefore it is built around a [`Message`](/src/Azos/Log/Message.cs) sealed class. `Message` is BSON-serializable for long-term storage.

**Messages header properties have the following cardinality:**

1. **Channel**: string - messages are processed by channels, e.g. "Security", "Social" etc..
2. **Topic** - within a channel messages are split by topics. Topics are to be taken from non-localizable constants, e.g. "RPC", "financial" etc.. Topics represents logical subs-systems/areas.
3. **From**: string - designates where message emitted from within the topic, aka. 'ComponentName'
4. **Source**: int - tracepoint/line# within component

The following describes `Message` properties:

<table>
<tr><th>Property</th><th>Description</th></tr>

<tr>
  <td>Guid <br><sup>Guid</sup></td>
  <td> Provides global unique identifier for this log message instance</td>
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
  <td>Topic <br><sup>string</sup></td>
  <td> 
   Gets/Sets a message topic/relation - the name of software concern within a big app, e.g. "Database", "Security" ...
   The `From` message property provides more detailed location under the specified `Topic`
 </td>
</tr>

<tr>
  <td>From <br><sup>string</sup></td>
  <td> Gets/Sets logical component ID, such as: class name, method name, process instance, that generated the message.
    This field is used in the scope of Topic </td>
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

<tr>
  <td>Channel <br><sup>string</sup></td>
  <td> 
   Gets/Sets logical partition name for messages. This property is usually used in Archive for splitting destinations
  </td>
</tr>

</table>

======================================

# Samples:

## Insert Log Batch Example:

The API method inserts one or more entries into the Chronicle logging systems.

**Request:**

```json
POST http://{host}:{port}/chronicle/log/batch HTTP/1.2
Authorization: {Base64 username:pwd}
Accept: application/json
Content-Type: application/json 

{
      "batch":{
            "data": [
                  {
                        "app": "irest",
                        "channel": "demo",
                        "type": "Warning", 
                        "topic": "RonaldReagan",
                        "from": "console", 
                        "text": "Glory to soviet caosmnaunts",
                        "source": 1234,
                        "host": "/g8/cle/fictionl",
                        "parameters": null,
                        "exceptiondata": null,
                        "archivedimensions": null
                  },

            ]
      }
}
```

**Response:**

```json
{
  "OK": true,
  "change": "Inserted",
  "status": 0,
  "affected": 1,
  "message": "Done in 6 ms",
  "data": null
}
```

======================================

## Filter Logs

The API method returns a filtered collection of log messages based on the provided search criteria.

**Request:**

```json
POST http://{host}:{port}/chronicle/log/filter  HTTP/1.2
Authorization: Basic {Base64 username:pwd}
Accept: application/json
Content-Type: application/json 

{
    "filter": {
        "Application": "hub",
        "MinType": "Debug",
        "MaxType": "CatastrophicError",
        "TimeRange": {
            "start": "2021-10-12T12:00:00.000Z",
            "end": "2021-10-13T12:00:00.000Z"
        },
        "PagingCount": 2,
        "CrossShard": true
    }
}
```

**Request (using Javascript):**

```javascript
fetch("http://{host}:{port}/chronicle/log/filter", {
  "headers": {
    "accept": "application/json",
    "accept-language": "en-US,en;q=0.9",
    "authorization": "Basic {Base64 username:pwd}",
    "cache-control": "max-age=0",
    "content-type": "application/json"
  },
  "referrer": "http://{host}:{port}/chronicle/log/view",
  "referrerPolicy": "strict-origin-when-cross-origin",
  "body": "{\"filter\":{\"Application\":\"hub\",\"MinType\":\"Debug\",\"MaxType\":\"CatastrophicError\",\"TimeRange\":{\"start\":\"2021-10-12T12:00:00.000Z\",\"end\":\"2021-10-13T12:00:00.000Z\"},\"PagingCount\":25,\"CrossShard\":true}}",
  "method": "POST",
  "mode": "cors",
  "credentials": "include"
});
```

**Response:**

```json
{
  "data": [
    {
      "Gdid": "0:4:16930",
      "Guid": "046e3345-f6fb-4fbb-99dc-73ac8f883f60",
      "App": "hub",
      "Type": "Info",
      "Source": 204,
      "UTCTimeStamp": "2021-10-12T12:09:05.860Z",
      "Host": "DEV1",
      "From": "DeleteFilesJob@136.DoFire",
      "Topic": "Time",
      "Text": "Scanned 58 files, 0 dirs; Deleted 0 files, 0 dirs"
    },
    {
      "Gdid": "0:6:15534",
      "Guid": "bc9f8fc9-5ac0-4efb-97e9-928012fa0ee0",
      "App": "hub",
      "Type": "Info",
      "Source": 204,
      "UTCTimeStamp": "2021-10-12T14:39:24.101Z",
      "Host": "DEV1",
      "From": "DeleteFilesJob@136.DoFire",
      "Topic": "Time",
      "Text": "Scanned 58 files, 0 dirs; Deleted 0 files, 0 dirs"
    },
    {
      "Gdid": "0:6:15538",
      "Guid": "9655cef2-8cfd-43d2-83a9-047b90855d14",
      "App": "hub",
      "Type": "Info",
      "Source": 204,
      "UTCTimeStamp": "2021-10-12T14:39:26.788Z",
      "Host": "DEV2",
      "From": "DeleteFilesJob@136.DoFire",
      "Topic": "Time",
      "Text": "Scanned 58 files, 0 dirs; Deleted 0 files, 0 dirs"
    }
  ],
  "OK": true
}
```

**Response (with no results):**

```json
{
  "data": [],
  "OK": true
}
```

======================================

