# Archiving

Back to [Documentation Index](/src/documentation-index.md)

Azos Archiving library facilitates writing and reading very large binary streams of 
structured data entries such as log/instrumentation messages and other business objects.

The library was engineered with the following key requirements:
- Archives may contain **very many (billions) of entries** of various sizes (tiny data messages to multi-megabyte multimedia content)
- Data is accessed via a **random-access `Stream`** which **must be append-only**; a `Stream` abstracts any kind of storage (such as: local file, Virtual FileSystem, 3rd party storage bucket etc.)
- Archive stream data is **resilient to partial corruption** (e.g. during transmission) and unaffected stream regions must be still readable
- The data might need to be optionally **compressed** and/or **encrypted** (with configurable schemes)
- Block chain storage
- Optional authenticity check (e.g. using hashes)
- The archive entry format must be flexible and efficient:
   - Entries may need to be polymorphic (different types)
   - Entries may use **custom serialization**, such as binary for efficiency
   - Entries may be as simple as a flat strings, or as complex as object graphs

Typical archive use-cases:
- Log message archiving, **distributed log warehousing**
- Telemetry/Instrumentation data archiving, distributed cluster **telemetry data warehousing**
- Application operation journal storage (such as business transaction log)
- **Data Warehousing** and **Big-data Analysis**
- **Feature indexing** aka **Columnar storage** - created during archive generation with minimal cost, significantly reduces data access needed for later analysis
- **High performance message streams** (like Kafka)

 In comparison with typical NoSQL/RBMS-backed archiving solutions, Azos archiving provides much better performance due to simplicity 
 and general architectural approach to using append-only "volumes" of data.

## Architecture

Archive data is accessed via an instances of `Stream`. This allows for decoupling from any specific data storage.
The stream has to satisfy the following requirements:
- Random-access for reading `stream.CanRead` and `stream.CanSeek` should be supported
- Append only writing `stream.CanWrite` should be true
- Should be able to return length `(long stream.Length)`

Archive data is accessed via an [IVolume](IVolume.cs) abstraction, having a [DefaultVolume](DefaultVolume.cs) provide
a base implementation. The volume data is accessed in units of data called [Pages](Page.cs). An archive is nothing but a
list of pages, starting with a header which contains metadata:
```
 [Archive Header + Metadata]
 [Page 1][Page 2]  ... [PageX]<eof>
```
> **Archives are by design append-only. All of the existing archive data is always immutable**. If one needs to alter an
> exiting archived data, that existing archive volume needs to be re-streamed into a new one making necessary modifications
> in-flight. *(Example: on a machine with SSD and Core i7 It takes less than 3 minutes to build an index for a 36 GB log archive containing 64 million messages)*

Archive data access is on a page-by-page basis as facilitated by an `IVolume` implementation. If compression and/or encryption is 
enabled then data transformations are applied per page. Page size is set via `IVolume.PageSizeBytes`. It only affects newly created 
pages when data is being added to archives.

> An archive may be **logically described as an `IEnumerable<Page>`**, having a `Page` modeled as `IEnumerable<TEntry>` consequently
> all archive reading applications are based on an already familiar LINQ(to objects) concepts

If data corruption happens, page data integrity is violated and affected pages may not be read-back, however the data enumeration
is recoverable because **pages are always stored aligned on a 16-byte boundary**, and each page has a header which pins the page to 
its physical location in an archive, thus providing more "cheap security" for page headers.

A page, in turn, has a similar layout but entry headers are NOT aligned to boundaries in order to save space. It is possible
to enumerate page `Entries` getting valid array segments representing entry raw data, or invalid flag in case of corruption.
When entries are added to page using a [ArchiveAppender](ArchiveAppender.cs)-derivative, the `IVolume.PageSizeBytes` is checked,
and if the items goes over, the next page is created. It is possible to have entries which are larger than page size, so the
page size is not a limit - it is merely a relative hint telling the appender when to split pages.

> When compression and/or encryption are enabled, the physical page size may be less (or more) than the one being actually written into archive volume, for example 
> a 16 kb page filled with textual data may trigger a page split, but compressed copy will only add 8 kb of data to archive

> The system performs **buffer size checks** in order to protect from **buffer overrun attacks**,
>  and ascertain message integrity to avoid inadvertent corruption. See [Format](Format.cs) for limits





## Archive Format

An archive is a sequentially written append-only stream of items. Archives container file format
is binary by design. Concrete archive implementations define the type of serialization used for
storing and retrieving individual items.

Archive file has the following format:
```bash
 <FILE-HEADER><00><00><info: string><metedata: jsonstring><pad: 35 0x20> <pages>
 
 FILE-HEADER: ASCII String = "#!/usr/bin/env bix\n\00";
 metadata: string Azos config JSON
    <len: beint32><utf8-bytes>  len = byte length of UTF8-encoded string containing config vector
 pages:
    [<page>..[<page>]]<eof>

 page:
   Pages are always aligned by 16
   PAGE-HDR = ASCII(`PG`)
   <PAGE-HDR><position: word><utcCreateDate: unixBElong><host: utf8string><app: atomBElong><len: beInt><entry-stream>  <eof|page-hdr>
             |-------- iv -----------------------------| - used for encryption             |compressed|
 entry-stream:
   [entry..[entry] <eof | TERMINATOR-ENTRY-HDR>

 entry:
   ENTRY-HDR = ASCII(`@>`) or  TERMINATOR-ENTRY-HDR = `\x00x00`
   <ENTRY-HDR><entry-len: varint> <entry-content: byte[entry-len]>
      2 bytes     1..9 bytes             1 byte...X        Shortest message:  4 bytes
   entry-len: varlong(e.g. LEB128) < MAX-ITEM-SZ configurable (16 MByte by default)
```

## Compression & Encryption
Compression and Encryption of the archived data **may not** be handled by the plain CompressionStream implementor 
because the Stream used by ArchiveReader needs to be seek-able. For that purpose, the archive data is compressed and 
ciphered on a page level, however that is not the job of ArchiveWriter class. 
Pages are always 16-byte boundary aligned....WIP

---
Back to [Documentation Index](/src/documentation-index.md)



## Design Notes

```csharp
var fs = m_FileSession.GetFile("tran-2021-01-10.lar").GetStream();
var data = VFSystemAccessor.Open(fs);
//var data = VFSystemAccessor.Create(fs, ArchiveMetadata meta);//,  cmp: ComnpressionType.None, enc: EncryptorType.AES);
var dataReader = new ErxTraxArchiveReader(data, new ArchivePtr(0));


var fs2 = m_FileSession.GetFile("tran-2021-01-10.mcr.lix").GetStream();
var idxMcr = new VFSystemFormat(fs2);
var idxMcrReader = new StringIndexArchiveReader(idxMcr, new ArchivePtr(0));


var totalCount = 0;
var abrogatedCount = 0;

//parallel read is used in CPU-intensive operations
var gang = new ParallelReader(() => new ErxTraxArchiveReader(data, new ArchivePtr(0));


Parallel.ForEach( entry => {  
  gang.RestartAt(entry.Pointer);
  var tx = gang.FirstOrDefault();
  if (tx!=null)
  {
    totalCount++;
    if (tx.Insurer.Medicare.CoverageLimit < 789.12M) 
      abrogatedCount++; 
  }
});

foreach(var entry in idxMcrReader)
{
  dataReader.RestartAt(entry.Pointer);
  var tx = dataReader.FirstOrDefault();
  if (tx!=null)
  {
    totalCount++;
    if (tx.Insurer.Medicare.CoverageLimit < 789.12M) 
      abrogatedCount++; 
  }
}

return (totalCount, abrogatedCount);
```