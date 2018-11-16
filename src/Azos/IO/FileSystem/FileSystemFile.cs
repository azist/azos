/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using Azos.Conf;

namespace Azos.IO.FileSystem
{
    /// <summary>
    /// Represents a file in a file system. This class is NOT thread-safe
    /// </summary>
    public sealed class FileSystemFile : FileSystemSessionItem
    {
      #region .ctor

        /// <summary>
        /// Internal method that should not be called by developers
        /// </summary>
        public FileSystemFile(FileSystemSession session,
                                        string parentPath,
                                        string name,
                                        IFileSystemHandle handle) :
                              base(session, parentPath, name, handle)
        {

        }

        protected override void Destructor()
        {
          if (m_FileStream!=null)
          {
            m_FileStream.Dispose();
            m_FileStream = null;
          }
          base.Destructor();
        }

      #endregion

      #region Fields

        private FileSystemStream m_FileStream;

      #endregion

      #region Properties

        /// <summary>
        /// Returns a stream usable for file content access. If the file has not opened stream yet it will be opened and cached
        /// </summary>
        public FileSystemStream FileStream
        {
          get
          {
            CheckDisposed();
            return m_FileStream ?? (m_FileStream = FileSystem.DoGetFileStream(this, (s) => m_FileStream = null));
          }
        }

      #endregion

      #region Public Sync Methods

        /// <summary>
        /// Reads all text from file using byte order mark detection with UTF8 encoding
        /// </summary>
        public string ReadAllText()
        {
          using(var fs = this.FileStream)
            using(var r = new StreamReader(fs))
              return r.ReadToEnd();
        }

                /// <summary>
                /// Async version of <see cref="ReadAllText()"/>
                /// </summary>
                public Task<string> ReadAllTextAsync()
                {
                  using(var fs = this.FileStream)
                  using(var r = new StreamReader(fs))
                    return r.ReadToEndAsync();
                }

        /// <summary>
        /// Reads all text from stream using the specified parameters
        /// </summary>
        public string ReadAllText(Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize = 1024)
        {
          using(var fs = this.FileStream)
            using(var r = new StreamReader(fs, encoding, detectEncodingFromByteOrderMarks, bufferSize))
              return r.ReadToEnd();
        }

                /// <summary>
                /// Async version of <see cref="ReadAllText(Encoding, bool, int)"/>
                /// </summary>
                public Task<string> ReadAllTextAsync(Encoding encoding, bool detectEncodingFromByteOrderMarks, int bufferSize = 1024)
                {
                  using(var fs = this.FileStream)
                  using(var r = new StreamReader(fs, encoding, detectEncodingFromByteOrderMarks, bufferSize))
                    return r.ReadToEndAsync();
                }

        /// <summary>
        /// Sets file content to supplied string using default UTF8 encoding
        /// </summary>
        public void WriteAllText(string text)
        {
          using(var fs = this.FileStream)
           using(var w = new StreamWriter(fs))
              w.Write(text);
        }

                /// <summary>
                /// Async version of <see cref="WriteAllText(string)"/>
                /// </summary>
                public Task WriteAllTextAsync(string text)
                {
                  using(var fs = this.FileStream)
                  using(var w = new StreamWriter(fs))
                    return w.WriteAsync(text);
                }

        /// <summary>
        /// Sets file content to supplied string using the specified parameters
        /// </summary>
        public void WriteAllText(string text, Encoding encoding, int bufferSize = 1024)
        {
          using(var fs = this.FileStream)
           using(var w = new StreamWriter(fs, encoding, bufferSize))
              w.Write(text);
        }

                /// <summary>
                /// Async version of <see cref="WriteAllText(string, Encoding, int)"/>
                /// </summary>
                public Task WriteAllTextAsync(string text, Encoding encoding, int bufferSize = 1024)
                {
                  using(var fs = this.FileStream)
                  using(var w = new StreamWriter(fs, encoding, bufferSize))
                    return w.WriteAsync(text);
                }

      #endregion
    }
}
