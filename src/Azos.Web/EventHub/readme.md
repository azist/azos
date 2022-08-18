# Event Hub

Provides event notification services.

The system was designed to avoid vendor lock-in and use in business oriented 1:M event 
notification such as micro service-based business architectures.

The **producers send messages to named "namespace/queues"** which can optionally be partitioned by `ShardKey`.
The message sequencing by time is preserved.

**Consumers poll/read the messages from distributed queue nodes** in cluster region. The consumer keeps track what
it has already read. A consumer can restart processing. In this regard, the design is similar to Apache Kafka.

The system is **multi-master** within cluster origin AND cross-region.
An **optional cross-origin replication service allows for multi-region multi-master replication** however 
the ordering of produced messages may be out-of-sequence between messages produced in different regions.

The system has the following traits:

- Unidirectional append only distributed, fault tolerant commit log of events
- An event is a `byte[]` with content type and headers
- Logical event sequence preserved within origin (cluster region)
- Leader-less design, multi-master per cluster region (origin)
- Fault tolerant with 1+ queue node copies in the cluster
- Avoids vendor lock-in (when using default server implementation)
- Server broker backend plug-and-play, default uses Mongo Db collections, can use MySql, Kafka, etc.
- Simple implementation, very few parameters to configure 
- Multi-region /multi-master option with replication agent