/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.IO.FileSystem
{
  /// <summary>
  /// General Azos file system specific exception
  /// </summary>
  [Serializable]
  public class FileSystemException: AzosException
  {
    public FileSystemException() { }
    public FileSystemException(string message) : base(message) { }
    public FileSystemException(string message, Exception inner) : base(message, inner) { }
    protected FileSystemException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  } //FileSystemException
}
