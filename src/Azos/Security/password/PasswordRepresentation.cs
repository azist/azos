/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;


namespace Azos.Security
{
  /// <summary>
  /// Flags denote types of password representation: Text/Image/Audio
  /// </summary>
  [Flags]
  public enum PasswordRepresentationType
  {
    Text = 1 << 0,
    Image = 1 << 1,
    Audio = 1 << 2
  }

  /// <summary>
  /// Provides password representation content, i.e. an image with drawn password which is understandable by humans
  /// </summary>
  public class PasswordRepresentation
  {
    public PasswordRepresentation(PasswordRepresentationType type, string contentType, byte[] content)
    {
      m_Type = type;
      m_ContentType = contentType;
      m_Content = content;
    }

    private PasswordRepresentationType m_Type;
    private string m_ContentType;
    private byte[] m_Content;

    public PasswordRepresentationType Type { get { return m_Type; } }
    public string ContentType { get { return m_ContentType; } }
    public byte[] Content { get { return m_Content; } }
  }
}
