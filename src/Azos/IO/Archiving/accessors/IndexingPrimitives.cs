/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;
using Azos.Financial;
using Azos.Serialization.Bix;
using Azos.Time;

namespace Azos.IO.Archiving
{
  #region STRING
  /// <summary>
  /// A tuple of (STRING:Bookmark) used for indexing archived data on string index value
  /// </summary>
  public struct StringBookmark : IEquatable<StringBookmark>
  {
    public StringBookmark(string value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly string Value;
    public readonly Bookmark Bookmark;

    public bool Assigned => Bookmark.Assigned;

    public bool Equals(StringBookmark other) => this.Value.EqualsOrdSenseCase(other.Value) && this.Bookmark.Equals(other.Bookmark);
    public override bool Equals(object obj) => obj is StringBookmark other ? this.Equals(other) : false;
    public override int GetHashCode() => (Value != null ? Value.GetHashCode() : 0) ^ Bookmark.GetHashCode();
    public override string ToString() => $"`{Value}` -> {Bookmark}";

    public static bool operator ==(StringBookmark l, StringBookmark r) => l.Equals(r);
    public static bool operator !=(StringBookmark l, StringBookmark r) => !l.Equals(r);
  }

  /// <summary>
  /// Reads StringBookmark index entries. The instance is thread safe
  /// </summary>
  public sealed class StringIdxReader : ArchiveBixReader<StringBookmark>
  {
    public StringIdxReader(IVolume volume) : base(volume){ }

    public override StringBookmark MaterializeBix(BixReader reader)
    {
      var v = reader.ReadString();
      var pid = reader.ReadLong();
      var adr = reader.ReadInt();
      return new StringBookmark(v, new Bookmark(pid, adr));
    }
  }

  /// <summary>
  /// Appends StringBookmarks to index. The instance is NOT thread-safe
  /// </summary>
  public sealed class StringIdxAppender : ArchiveBixAppender<StringBookmark>
  {
    public StringIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<StringBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit){ }

    protected override void DoSerializeBix(BixWriter wri, StringBookmark entry)
    {
      wri.Write(entry.Value);
      wri.Write(entry.Bookmark.PageId);
      wri.Write(entry.Bookmark.Address);
    }
  }
  #endregion


  #region GUID
  /// <summary>
  /// A tuple of (Guid:Bookmark) used for indexing archived data on guid index value
  /// </summary>
  public struct GuidBookmark : IEquatable<GuidBookmark>
  {
    public GuidBookmark(Guid value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly Guid Value;
    public readonly Bookmark Bookmark;

    public bool Assigned => Bookmark.Assigned;

    public bool Equals(GuidBookmark other) => this.Value.Equals(other.Value) && this.Bookmark.Equals(other.Bookmark);
    public override bool Equals(object obj) => obj is GuidBookmark other ? this.Equals(other) : false;
    public override int GetHashCode() => Value.GetHashCode() ^ Bookmark.GetHashCode();
    public override string ToString() => $"`{Value}` -> {Bookmark}";

    public static bool operator ==(GuidBookmark l, GuidBookmark r) => l.Equals(r);
    public static bool operator !=(GuidBookmark l, GuidBookmark r) => !l.Equals(r);
  }


  /// <summary>
  /// Reads GuidBookmark index entries. The instance is thread safe
  /// </summary>
  public sealed class GuidIdxReader : ArchiveBixReader<GuidBookmark>
  {
    public GuidIdxReader(IVolume volume) : base(volume) { }

    public override GuidBookmark MaterializeBix(BixReader reader)
    {
      var v = reader.ReadGuid();
      var pid = reader.ReadLong();
      var adr = reader.ReadInt();
      return new GuidBookmark(v, new Bookmark(pid, adr));
    }
  }

  /// <summary>
  /// Appends GuidBookmarks to index. The instance is NOT thread-safe
  /// </summary>
  public sealed class GuidIdxAppender : ArchiveBixAppender<GuidBookmark>
  {
    public GuidIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<GuidBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit) { }

    protected override void DoSerializeBix(BixWriter wri, GuidBookmark entry)
    {
      wri.Write(entry.Value);
      wri.Write(entry.Bookmark.PageId);
      wri.Write(entry.Bookmark.Address);
    }
  }

  #endregion


  #region GDID
  /// <summary>
  /// A tuple of (GDID:Bookmark) used for indexing archived data on gdid index value
  /// </summary>
  public struct GdidBookmark : IEquatable<GdidBookmark>
  {
    public GdidBookmark(GDID value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly GDID Value;
    public readonly Bookmark Bookmark;

    public bool Assigned => Bookmark.Assigned;

    public bool Equals(GdidBookmark other) => this.Value.Equals(other.Value) && this.Bookmark.Equals(other.Bookmark);
    public override bool Equals(object obj) => obj is GdidBookmark other ? this.Equals(other) : false;
    public override int GetHashCode() => Value.GetHashCode() ^ Bookmark.GetHashCode();
    public override string ToString() => $"`{Value}` -> {Bookmark}";

    public static bool operator ==(GdidBookmark l, GdidBookmark r) => l.Equals(r);
    public static bool operator !=(GdidBookmark l, GdidBookmark r) => !l.Equals(r);
  }


  /// <summary>
  /// Reads GdidBookmark index entries. The instance is thread safe
  /// </summary>
  public sealed class GdidIdxReader : ArchiveBixReader<GdidBookmark>
  {
    public GdidIdxReader(IVolume volume) : base(volume) { }

    public override GdidBookmark MaterializeBix(BixReader reader)
    {
      var v = reader.ReadGDID();
      var pid = reader.ReadLong();
      var adr = reader.ReadInt();
      return new GdidBookmark(v, new Bookmark(pid, adr));
    }
  }

  /// <summary>
  /// Appends GdidBookmarks to index. The instance is NOT thread-safe
  /// </summary>
  public sealed class GdidIdxAppender : ArchiveBixAppender<GdidBookmark>
  {
    public GdidIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<GdidBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit) { }

    protected override void DoSerializeBix(BixWriter wri, GdidBookmark entry)
    {
      wri.Write(entry.Value);
      wri.Write(entry.Bookmark.PageId);
      wri.Write(entry.Bookmark.Address);
    }
  }

  #endregion


  #region DATETIME
  /// <summary>
  /// A tuple of (DateTime:Bookmark) used for indexing archived data on datetime index value
  /// </summary>
  public struct DateTimeBookmark : IEquatable<DateTimeBookmark>
  {
    public DateTimeBookmark(DateTime value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly DateTime Value;
    public readonly Bookmark Bookmark;

    public bool Assigned => Bookmark.Assigned;

    public bool Equals(DateTimeBookmark other) => this.Value.Equals(other.Value) && this.Bookmark.Equals(other.Bookmark);
    public override bool Equals(object obj) => obj is DateTimeBookmark other ? this.Equals(other) : false;
    public override int GetHashCode() => Value.GetHashCode() ^ Bookmark.GetHashCode();
    public override string ToString() => $"`{Value}` -> {Bookmark}";

    public static bool operator ==(DateTimeBookmark l, DateTimeBookmark r) => l.Equals(r);
    public static bool operator !=(DateTimeBookmark l, DateTimeBookmark r) => !l.Equals(r);
  }


  /// <summary>
  /// Reads DateTimeBookmark index entries. The instance is thread safe
  /// </summary>
  public sealed class DateTimeIdxReader : ArchiveBixReader<DateTimeBookmark>
  {
    public DateTimeIdxReader(IVolume volume) : base(volume) { }

    public override DateTimeBookmark MaterializeBix(BixReader reader)
    {
      var v = reader.ReadDateTime();
      var pid = reader.ReadLong();
      var adr = reader.ReadInt();
      return new DateTimeBookmark(v, new Bookmark(pid, adr));
    }
  }

  /// <summary>
  /// Appends DateTimeBookmarks to index. The instance is NOT thread-safe
  /// </summary>
  public sealed class DateTimeIdxAppender : ArchiveBixAppender<DateTimeBookmark>
  {
    public DateTimeIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<DateTimeBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit) { }

    protected override void DoSerializeBix(BixWriter wri, DateTimeBookmark entry)
    {
      wri.Write(entry.Value);
      wri.Write(entry.Bookmark.PageId);
      wri.Write(entry.Bookmark.Address);
    }
  }

  #endregion


  #region LONG
  /// <summary>
  /// A tuple of (Long:Bookmark) used for indexing archived data on long index value
  /// </summary>
  public struct LongBookmark : IEquatable<LongBookmark>
  {
    public LongBookmark(long value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly long Value;
    public readonly Bookmark Bookmark;

    public bool Assigned => Bookmark.Assigned;

    public bool Equals(LongBookmark other) => this.Value.Equals(other.Value) && this.Bookmark.Equals(other.Bookmark);
    public override bool Equals(object obj) => obj is LongBookmark other ? this.Equals(other) : false;
    public override int GetHashCode() => Value.GetHashCode() ^ Bookmark.GetHashCode();
    public override string ToString() => $"`{Value}` -> {Bookmark}";

    public static bool operator ==(LongBookmark l, LongBookmark r) => l.Equals(r);
    public static bool operator !=(LongBookmark l, LongBookmark r) => !l.Equals(r);
  }


  /// <summary>
  /// Reads LongBookmark index entries. The instance is thread safe
  /// </summary>
  public sealed class LongIdxReader : ArchiveBixReader<LongBookmark>
  {
    public LongIdxReader(IVolume volume) : base(volume) { }

    public override LongBookmark MaterializeBix(BixReader reader)
    {
      var v = reader.ReadLong();
      var pid = reader.ReadLong();
      var adr = reader.ReadInt();
      return new LongBookmark(v, new Bookmark(pid, adr));
    }
  }

  /// <summary>
  /// Appends LongBookmarks to index. The instance is NOT thread-safe
  /// </summary>
  public sealed class LongIdxAppender : ArchiveBixAppender<LongBookmark>
  {
    public LongIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<LongBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit) { }

    protected override void DoSerializeBix(BixWriter wri, LongBookmark entry)
    {
      wri.Write(entry.Value);
      wri.Write(entry.Bookmark.PageId);
      wri.Write(entry.Bookmark.Address);
    }
  }

  #endregion


  #region DOUBLE
  /// <summary>
  /// A tuple of (Double:Bookmark) used for indexing archived data on double index value
  /// </summary>
  public struct DoubleBookmark : IEquatable<DoubleBookmark>
  {
    public DoubleBookmark(double value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly double Value;
    public readonly Bookmark Bookmark;

    public bool Assigned => Bookmark.Assigned;

    public bool Equals(DoubleBookmark other) => this.Value.Equals(other.Value) && this.Bookmark.Equals(other.Bookmark);
    public override bool Equals(object obj) => obj is DoubleBookmark other ? this.Equals(other) : false;
    public override int GetHashCode() => Value.GetHashCode() ^ Bookmark.GetHashCode();
    public override string ToString() => $"`{Value}` -> {Bookmark}";

    public static bool operator ==(DoubleBookmark l, DoubleBookmark r) => l.Equals(r);
    public static bool operator !=(DoubleBookmark l, DoubleBookmark r) => !l.Equals(r);
  }


  /// <summary>
  /// Reads DoubleBookmark index entries. The instance is thread safe
  /// </summary>
  public sealed class DoubleIdxReader : ArchiveBixReader<DoubleBookmark>
  {
    public DoubleIdxReader(IVolume volume) : base(volume) { }

    public override DoubleBookmark MaterializeBix(BixReader reader)
    {
      var v = reader.ReadDouble();
      var pid = reader.ReadLong();
      var adr = reader.ReadInt();
      return new DoubleBookmark(v, new Bookmark(pid, adr));
    }
  }

  /// <summary>
  /// Appends DoubleBookmarks to index. The instance is NOT thread-safe
  /// </summary>
  public sealed class DoubleIdxAppender : ArchiveBixAppender<DoubleBookmark>
  {
    public DoubleIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<DoubleBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit) { }

    protected override void DoSerializeBix(BixWriter wri, DoubleBookmark entry)
    {
      wri.Write(entry.Value);
      wri.Write(entry.Bookmark.PageId);
      wri.Write(entry.Bookmark.Address);
    }
  }

  #endregion


  #region DECIMAL
  /// <summary>
  /// A tuple of (Decimal:Bookmark) used for indexing archived data on decimal index value
  /// </summary>
  public struct DecimalBookmark : IEquatable<DecimalBookmark>
  {
    public DecimalBookmark(decimal value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly decimal Value;
    public readonly Bookmark Bookmark;

    public bool Assigned => Bookmark.Assigned;

    public bool Equals(DecimalBookmark other) => this.Value.Equals(other.Value) && this.Bookmark.Equals(other.Bookmark);
    public override bool Equals(object obj) => obj is DecimalBookmark other ? this.Equals(other) : false;
    public override int GetHashCode() => Value.GetHashCode() ^ Bookmark.GetHashCode();
    public override string ToString() => $"`{Value}` -> {Bookmark}";

    public static bool operator ==(DecimalBookmark l, DecimalBookmark r) => l.Equals(r);
    public static bool operator !=(DecimalBookmark l, DecimalBookmark r) => !l.Equals(r);
  }


  /// <summary>
  /// Reads DecimalBookmark index entries. The instance is thread safe
  /// </summary>
  public sealed class DecimalIdxReader : ArchiveBixReader<DecimalBookmark>
  {
    public DecimalIdxReader(IVolume volume) : base(volume) { }

    public override DecimalBookmark MaterializeBix(BixReader reader)
    {
      var v = reader.ReadDecimal();
      var pid = reader.ReadLong();
      var adr = reader.ReadInt();
      return new DecimalBookmark(v, new Bookmark(pid, adr));
    }
  }

  /// <summary>
  /// Appends DecimalBookmarks to index. The instance is NOT thread-safe
  /// </summary>
  public sealed class DecimalIdxAppender : ArchiveBixAppender<DecimalBookmark>
  {
    public DecimalIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<DecimalBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit) { }

    protected override void DoSerializeBix(BixWriter wri, DecimalBookmark entry)
    {
      wri.Write(entry.Value);
      wri.Write(entry.Bookmark.PageId);
      wri.Write(entry.Bookmark.Address);
    }
  }

  #endregion


  #region AMOUNT
  /// <summary>
  /// A tuple of (Amount:Bookmark) used for indexing archived data on amount (decimal with currency ISO code) index value
  /// </summary>
  public struct AmountBookmark : IEquatable<AmountBookmark>
  {
    public AmountBookmark(Amount value, Bookmark bm)
    {
      Value = value;
      Bookmark = bm;
    }

    public readonly Amount Value;
    public readonly Bookmark Bookmark;

    public bool Assigned => Bookmark.Assigned;

    public bool Equals(AmountBookmark other) => this.Value.Equals(other.Value) && this.Bookmark.Equals(other.Bookmark);
    public override bool Equals(object obj) => obj is AmountBookmark other ? this.Equals(other) : false;
    public override int GetHashCode() => Value.GetHashCode() ^ Bookmark.GetHashCode();
    public override string ToString() => $"`{Value}` -> {Bookmark}";

    public static bool operator ==(AmountBookmark l, AmountBookmark r) => l.Equals(r);
    public static bool operator !=(AmountBookmark l, AmountBookmark r) => !l.Equals(r);
  }


  /// <summary>
  /// Reads AmountBookmark index entries. The instance is thread safe
  /// </summary>
  public sealed class AmountIdxReader : ArchiveBixReader<AmountBookmark>
  {
    public AmountIdxReader(IVolume volume) : base(volume) { }

    public override AmountBookmark MaterializeBix(BixReader reader)
    {
      var v = reader.ReadAmount();
      var pid = reader.ReadLong();
      var adr = reader.ReadInt();
      return new AmountBookmark(v, new Bookmark(pid, adr));
    }
  }

  /// <summary>
  /// Appends AmountBookmarks to index. The instance is NOT thread-safe
  /// </summary>
  public sealed class AmountIdxAppender : ArchiveBixAppender<AmountBookmark>
  {
    public AmountIdxAppender(IVolume volume, ITimeSource time, Atom app, string host, Action<AmountBookmark, Bookmark> onPageCommit = null)
     : base(volume, time, app, host, onPageCommit) { }

    protected override void DoSerializeBix(BixWriter wri, AmountBookmark entry)
    {
      wri.Write(entry.Value);
      wri.Write(entry.Bookmark.PageId);
      wri.Write(entry.Bookmark.Address);
    }
  }
  #endregion

}
