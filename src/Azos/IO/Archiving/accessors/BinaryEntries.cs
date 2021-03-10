/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Time;
using Azos.Serialization.Bix;
using System.Collections.Generic;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Reads archives of byte[] entries. The implementation is thread-safe
  /// </summary>
  [ContentTypeSupport(BinaryArchiveAppender.CONTENT_TYPE_PATTERN_ANY_BIN)]
  public sealed class BinaryArchiveReader : ArchiveBixReader<byte[]>
  {
    public BinaryArchiveReader(IVolume volume) : base(volume){ }
    public override byte[] MaterializeBix(BixReader reader) => reader.ReadByteArray();
  }

  /// <summary>
  /// Appends byte[] entries to archive. The instance is NOT thread-safe
  /// </summary>
  [ContentTypeSupport(BinaryArchiveAppender.CONTENT_TYPE_BIN)]
  public sealed class BinaryArchiveAppender : ArchiveBixAppender<byte[]>
  {
    public const string CONTENT_TYPE_BIN = "bix/bin";
    public const string CONTENT_TYPE_PATTERN_ANY_BIN = "bix/bin*";

    /// <summary>
    /// Appends byte[] items to archive. The instance is NOT thread-safe
    /// </summary>
    public BinaryArchiveAppender(IVolume volume,
                                 ITimeSource time,
                                 Atom app,
                                 string host,
                                 Action<byte[], Bookmark> onPageCommit = null)
      : base(volume, time, app, host, onPageCommit) { }

    protected override void DoSerializeBix(BixWriter wri, byte[] entry)
      => wri.Write(entry);
  }
}
