/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Time;
using Azos.Serialization.Bix;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Reads archives of String entries. The implementation is thread-safe
  /// </summary>
  public sealed class StringArchiveReader : ArchiveBixReader<string>
  {
    public StringArchiveReader(IVolume volume) : base(volume){ }
    public override string MaterializeBix(BixReader reader) => reader.ReadString();
  }

  /// <summary>
  /// Appends string entries to archive. The instance is NOT thread-safe
  /// </summary>
  public sealed class StringArchiveAppender : ArchiveBixAppender<string>
  {
    /// <summary>
    /// Appends string items to archive. The instance is NOT thread-safe
    /// </summary>
    public StringArchiveAppender(IVolume volume,
                                 ITimeSource time,
                                 Atom app,
                                 string host,
                                 Action<string, Bookmark> onPageCommit = null)
      : base(volume, time, app, host, onPageCommit) { }


    protected override void DoSerializeBix(BixWriter wri, string entry)
      => wri.Write(entry);
  }
}
