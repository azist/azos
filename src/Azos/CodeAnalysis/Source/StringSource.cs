/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

namespace Azos.CodeAnalysis.Source
{
  /// <summary>
  /// Provides source code from a fully materialized string value
  /// </summary>
  public sealed class StringSource : ISourceText
  {
    private Language m_Language;
    private string m_Name;
    private string m_Source;
    private int m_Length;
    private int m_Position;

    public StringSource(string source, Language language = null, string name = null)
    {
      m_Source = source.NonNull(nameof(source));
      m_Language = language ?? UnspecifiedLanguage.Instance;
      m_Length = source.Length;
      m_Name = name ?? "<noname>";
    }

    /// <summary>
    /// Starts reading anew
    /// </summary>
    public StringSource Reset()
    {
      m_Position = 0;
      return this;
    }

    #region ISourceText Members
    public string Name => m_Name;
    public Language Language => m_Language;
    public System.Text.Encoding Encoding => System.Text.Encoding.Default;
    public bool EOF => m_Position >= m_Length;
    public char ReadChar() => m_Position >= m_Length ? (char)0 : m_Source[m_Position++];
    public char PeekChar() => m_Position >= m_Length ? (char)0 : m_Source[m_Position];
    public Task FetchSegmentAsync(System.Threading.CancellationToken ctk = default) => Task.CompletedTask;
    public int BufferSize => m_Source.Length;
    public int SegmentLength => m_Source.Length;
    public int SegmentPosition => m_Position;
    public bool NearEndOfSegment => false;
    public bool IsLastSegment => true;
    public int SegmentTailThreshold => 0;
    public int SegmentCount => 1;
    #endregion
  }
}

