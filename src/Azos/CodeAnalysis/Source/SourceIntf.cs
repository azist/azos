/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

namespace Azos.CodeAnalysis.Source
{

#warning AZ #731 rewrite async deserializer core - add CHUNKING ASYNC/SYNC block handling with "ChunkEof" which triggers next async fetch in chunks
  /// <summary>
  /// Represents source code input text (usually text from file)
  /// </summary>
  public interface ISourceText
  {
    /// <summary>
    /// Resets source to beginning
    /// </summary>
    void Reset();

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
    /// Indicates what language this source is supplied in
    /// </summary>
    Language Language { get; }

    /// <summary>
    /// Provides a meaningful name to a source code
    /// </summary>
    string Name { get; }
  }


  /// <summary>
  /// Represents a list of strings used as source text
  /// </summary>
  public class ListOfISourceText : List<ISourceText>
  {
  }
}
