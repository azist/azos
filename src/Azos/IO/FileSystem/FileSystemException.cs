
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
