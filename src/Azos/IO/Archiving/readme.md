# Archiving

Back to [Documentation Index](/src/documentation-index.md)

This library facilitates writing and reading data files containing sequential streams of items.
Used for log and instrumentation item warehousing and processing such as map reduce jobs.

## Format

An archive is a sequentially written append-only stream of items. Archives container file format
is binary by design. Concrete archive implementations define the type of serialization used for
storing and retrieving individual items.

Archive file has the following format:
```bash
 <FILE-HEADER> <metedata: json> <pages>
 
 FILE-HEADER: ASCII String = "#!/usr/bin/env bix\n\00";
 metadata: string Azos config JSON
    <len: beint32><utf8-bytes>  len = byte length of UTF8-encoded string containing config vector
 pages:
    [<page>..[<page>]]<eof>

 page:
   Pages are always aligned by 16
   PAGE-HDR = ASCII(`PAGE`)
   <PAGE-HDR><position: long><utcCreateDate: unixBElong><host: utf8string><app: atomBElong><entry-stream>  <eof|page-hdr>
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