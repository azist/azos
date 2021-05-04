/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Collections.Generic;

namespace Azos.CodeAnalysis.Source
{
  /// <summary>
  /// Provides source code from string
  /// </summary>
  public class StringSource : ISourceText
  {
    private Language m_Language;
    private string m_Name;
    private string m_Source;
    private int m_Length;
    private int m_Position;

    public StringSource(string source, Language language = null, string name = null)
    {
      m_Language = language;
      m_Source = source;
      m_Length = source.Length;
      m_Name = name;
    }

    #region ISourceText Members

    public void Reset() => m_Position = 0;

    public bool EOF => m_Position >= m_Length;

    public char ReadChar() => m_Position >= m_Length ? (char)0 : m_Source[m_Position++];

    public char PeekChar() => m_Position >= m_Length ? (char)0 : m_Source[m_Position];

    public Language Language => m_Language ?? UnspecifiedLanguage.Instance;

    /// <summary>
    /// Provides a handy way to name an otherwise-anonymous string source code,
    /// This property is like a "file name" only data is kept in a string
    /// </summary>
    public string Name => m_Name ?? string.Empty;

    #endregion
  }


  /// <summary>
  /// Represents a list of strings used as source text
  /// </summary>
  public class StringSourceList : List<StringSource>
  {

  }
}

