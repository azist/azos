/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Text;
using System.IO;

namespace Azos.CodeAnalysis.Source
{
  /// <summary>
  /// Represents source code stored in a file
  /// </summary>
  public sealed class FileSource : StreamSource
  {
    public const int MIN_FILE_BUFFER_SIZE = 1 * 1024;
    public const int MAX_FILE_BUFFER_SIZE = 64 * 1024;

    public const int DEFAULT_FILE_BUFFER_SIZE = 4 * 1024;

    /// <summary>
    /// Constructs file source inferring source language from file extension if not specified
    /// </summary>
    public FileSource(string fileName,
                      Encoding encoding = null,
                      Language language = null,
                      int bufferSize = 0,
                      int segmentTailThreshold = 0,
                      int fileBufferSize = 0,
                      bool sensitiveData = false) : base()
    {
      fileName.NonBlank(nameof(fileName));

      if (language==null)
      {
        language = Language.TryFindLanguageByFileExtension(Path.GetExtension(fileName));
      }

      if (fileBufferSize < 1) fileBufferSize = DEFAULT_FILE_BUFFER_SIZE;

      m_FileStream = new FileStream(fileName,
                                    FileMode.Open,
                                    FileAccess.Read,
                                    FileShare.Read,
                                    fileBufferSize.KeepBetween(MIN_FILE_BUFFER_SIZE, MAX_FILE_BUFFER_SIZE));

      ctor(m_FileStream, encoding, language, fileName, bufferSize, segmentTailThreshold, sensitiveData);
    }
    protected override void Destructor()
    {
      DisposeAndNull(ref m_FileStream);
      base.Destructor();
    }

    private FileStream m_FileStream;
  }
}
