/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Serialization.Bix;

namespace Azos.IO.Archiving
{
  /// <summary>
  /// Reads archives of String items. The implementation is thread-safe
  /// </summary>
  public sealed class StringArchiveReader : ArchiveReader<string>
  {
    public StringArchiveReader(IVolume volume) : base(volume){ }

    [ThreadStatic] private static BufferSegmentReadingStream ts_Stream;

    public override string Materialize(Entry entry)
    {
      if (entry.State != Entry.Status.Valid) return null;

      var stream = ts_Stream;
      if (stream == null)
      {
        stream = new BufferSegmentReadingStream();
        ts_Stream = stream;
      }

      stream.UnsafeBindBuffer(entry.Raw);
      var reader = new BixReader(stream);

      string result = reader.ReadString();

      stream.UnbindBuffer();

      return result;
    }
  }
}
