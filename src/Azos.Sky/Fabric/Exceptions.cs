/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
using System;
using System.Runtime.Serialization;


namespace Azos.Sky.Fabric
{
  /// <summary>
  /// Thrown to indicate fabric-related problems: fiber allocation, runtime etc
  /// </summary>
  [Serializable]
  public class FabricException : SkyException
  {
    public FabricException() : base() {}
    public FabricException(string message) : base(message) {}
    public FabricException(string message, Exception inner) : base(message, inner) { }
    protected FabricException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }

  /// <summary>
  /// Thrown to indicate inability to reserve space to allocate new fiber - seen as 404
  /// </summary>
  [Serializable]
  public class FabricFiberAllocationException : FabricException, IHttpStatusProvider
  {
    public FabricFiberAllocationException() : base() { }
    public FabricFiberAllocationException(string message) : base(message) { }
    public FabricFiberAllocationException(string message, Exception inner) : base(message, inner) { }
    protected FabricFiberAllocationException(SerializationInfo info, StreamingContext context) : base(info, context) { }

    public int HttpStatusCode => 404;
    public string HttpStatusDescription => "Fiber allocation space not found";
  }

  /// <summary>
  /// Thrown to indicate error conditions in processor nodes
  /// </summary>
  [Serializable]
  public class FabricProcessorException : FabricException
  {
    public FabricProcessorException() : base() { }
    public FabricProcessorException(string message) : base(message) { }
    public FabricProcessorException(string message, Exception inner) : base(message, inner) { }
    protected FabricProcessorException(SerializationInfo info, StreamingContext context) : base(info, context) { }
  }
}
