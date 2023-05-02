/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;
using System.Threading.Tasks;

namespace Azos.CodeAnalysis.Source
{
  /// <summary>
  /// Represents an abstraction of a source code text which comes from various sources such as strings, streams or files.
  /// The interface is designed for hybrid read model which uses sync code to <see cref="ReadChar"/> and <see cref="PeekChar"/>
  /// from an in-memory buffer which is filled asynchronously.
  /// The trick is to check <see cref="NearEndOfSegment"/> from async code and continue synchronous reading until the property returns true,
  /// in which case call <see cref="FetchSegmentAsync(System.Threading.CancellationToken)"/> asynchronously.
  /// This way the system performs efficient synchronous processing of segments which are fetched asynchronously, thus
  /// getting benefits of both models: sync performance and non-blocking async behavior with ability to process "infinite" content as it comes.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Represents a textual source stored in a stream which can be asynchronously read in segments
  /// which are then synchronously processed for performance. <br /><br/>
  /// This is a hybrid processing model which brings benefits of sync and async processing.
  /// This class is built for convenience and performance, as it is used by some deserializers (e.g. JSON)
  /// it must be able to efficiently process large bodies of source text, for example supplied via a network stream
  /// asynchronously. A naive async implementation with async `Read/PeekChar` would have been very inefficient, therefore
  /// this class provides a synchronous character-by-character read interface which is fed from internal memory segments
  /// which are pre-fetched asynchronously, therefore a large source input is still processed asynchronously in segments, each
  /// processed synchronously one-after another.
  /// </para>
  /// <para>
  /// The caller inspects boolean <see cref="NearEndOfSegment"/> to trigger async call to <see cref="FetchSegmentAsync"/>
  /// while the sync caller may not have ended reading a current segment which was previously fetched,
  /// this way it is possible to consume source char-by-char synchronously without any extra allocations
  /// and overhead associated with async processing, while synchronously (efficiently) looking at <see cref="NearEndOfSegment"/>
  /// property and triggering an asynchronous prefetch of the next segment which does not happen for every character.
  /// </para>
  /// <para>
  /// The <see cref="NearEndOfSegment"/> is basically a speculative property which returns true as soon as segment read index
  /// approaches the end of the segment as dictated by % margin near the segment end.
  /// It is possible that a sync operation may need to read more than what was fetched in which case it will trigger
  /// a blocking sync call on the async <see cref="FetchSegmentAsync"/>, however statistically this is a rare case.
  /// </para>
  /// </remarks>
  public interface ISourceText : Collections.INamed//Provides a meaningful name to a source code
  {
    /// <summary>
    /// Indicates what language this source is supplied in
    /// </summary>
    Language Language { get; }

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
    /// Encoding used for reading text
    /// </summary>
    System.Text.Encoding Encoding { get; }

    /// <summary>
    /// How much memory (in bytes) is used for buffering reads from source.
    /// At first the system reads the source Segment asynchronously into the pre-allocated buffer.
    /// Then, the system uses sync functions <see cref="ReadChar"/> and <see cref="PeekChar"/>
    /// to get character efficiently from memory.
    /// </summary>
    int BufferSize { get; }

    /// <summary>
    /// Specifies a character length relative to the end of the current segment beyond which the
    /// system deems the state as <see cref="NearEndOfSegment"/>
    /// </summary>
    int SegmentTailThreshold { get; }

    /// <summary>
    /// Character length of a pre-fetched character segment which was read from source into memory buffer,
    /// possibly asynchronously
    /// </summary>
    int SegmentLength { get; }

    /// <summary>
    /// Character position within a Segment which was read from source, possibly asynchronously.
    /// The caller may trigger an async fetch to get more data from the underlying source asynchronously
    /// when this number approaches <see cref="SegmentLength"/>.
    /// When this property goes past <see cref="SegmentLength"/> the system calls <see cref="FetchSegmentAsync"/>
    /// automatically, however you can check these properties yourself and trigger a fully async fetch yourself
    /// </summary>
    int SegmentPosition { get; }

    /// <summary>
    /// How many segments have been read so far
    /// </summary>
    int SegmentCount { get; }

    /// <summary>
    /// Returns true when implementation deems <see cref="SegmentPosition"/> is getting close enough to <see cref="SegmentLength"/>.
    /// Inspect this property to trigger async call to <see cref="FetchSegmentAsync"/>
    /// </summary>
    bool NearEndOfSegment { get; }

    /// <summary>
    /// True, if the fetched segment is the last one as the source has been read to the very end.
    /// The call to <see cref="FetchSegmentAsync"/> is not needed/does nothing
    /// </summary>
    bool IsLastSegment { get; }

    /// <summary>
    /// Fetches more - the next character segment into buffer asynchronously, then use synchronous functions to efficiently get data
    /// using sync <see cref="ReadChar"/> and <see cref="PeekChar"/>
    /// </summary>
    Task FetchSegmentAsync(System.Threading.CancellationToken ctk = default);
  }
}
