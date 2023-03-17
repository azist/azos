/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azos.CodeAnalysis.Source
{
#warning AZ #731 rewrite async deserializer core - add CHUNKING ASYNC/SYNC block handling with "ChunkEof" which triggers next async fetch in chunks
  /// <summary>
  /// Represents an abstraction of a source code text which comes from various sources such as strings, streams or files
  /// </summary>
  public interface ISourceText : Collections.INamed//Provides a meaningful name to a source code
  {
    /// <summary>
    /// Indicates what language this source is supplied in
    /// </summary>
    Language Language { get; }

    /// <summary>
    /// Resets source to the very beginning
    /// </summary>
  //  void Reset();

    /// <summary>
    /// Indicates whether last character has been read
    /// </summary>
    bool EOF { get; }

    /// <summary>
    /// Returns next char and advances position
    /// </summary>
    char ReadChar();

    /// <summary>
    /// Returns next char without advancing position
    /// </summary>
    char PeekChar();

    /// <summary>
    /// How much memory (in characters) is used for buffering reads from source.
    /// At first the system reads the source Segment asynchronously into the allocated buffer.
    /// Then, the system uses sync functions <see cref="ReadChar"/> and <see cref="PeekChar"/>
    /// to get character efficiently from memory.
    /// </summary>
    int BufferSize { get; }

    /// <summary>
    /// Character length of a pre-fetched character segment which was read from source into memory buffer,
    /// possibly asynchronously
    /// </summary>
    int SegmentLength { get; }

    /// <summary>
    /// Character position within a Segment which was read from source, possibly asynchronously.
    /// The caller may trigger an async fetch to get more data from the underlying source asynchronously
    /// when this number approaches <see cref="SegmentLength"/>.
    /// When this property goes past <see cref="SegmentLength"/> the system calls <see cref="FetchBufferAsync"/>
    /// automatically, however you can check these properties yourself and trigger a fully async fetch yourself
    /// </summary>
    int SegmentPosition { get; }

    /// <summary>
    /// Return true when implementation deems <see cref="SegmentPosition"/> get close to <see cref="SegmentLength"/>.
    /// Inspect this property to trigger async call to <see cref="FetchSegmentAsync"/>
    /// </summary>
    bool NearEndOfSegment { get; }

    /// <summary>
    /// True, if the fetched segment is the last one as the source has been read to the very end.
    /// The call to <see cref="FetchSegmentAsync"/> is not needed/does nothing
    /// </summary>
    bool IsLastSegment { get; }

    /// <summary>
    /// Fetches more - the next character segment into buffer asynchronously, then use synchronous function to efficiently get data
    /// using sync <see cref="ReadChar"/> and <see cref="PeekChar"/>
    /// </summary>
    ValueTask FetchSegmentAsync();
  }


  /// <summary>
  /// Represents a list of strings used as source text
  /// </summary>
  public class ListOfISourceText : List<ISourceText>
  {
  }
}
