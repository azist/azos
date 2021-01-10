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
   ENTRY-HDR = ASCII(`OK`) or  TERMINATOR-ENTRY-HDR = `\x00x00`
   <ENTRY-HDR><entry-len: varulong> <entry-content: byte[entry-len]>
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
