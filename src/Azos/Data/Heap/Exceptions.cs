/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;

namespace Azos.Data.Heap
{
  /// <summary>
  /// Base exception thrown by the data heap
  /// </summary>
  [Serializable]
  public class DataHeapException : DataException
  {
    public DataHeapException() { }
    public DataHeapException(string message) : base(message) { }
    public DataHeapException(string message, Exception inner) : base(message, inner) { }
    protected DataHeapException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}