using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Azos.Sky.Contracts
{

  /// <summary>
  /// Marker interface that denotes contracts that represent services
  /// </summary>
  public interface ISkyService { }

  /// <summary>
  /// Marker interface for service clients that get managed by ServiceClient class
  /// </summary>
  public interface ISkyServiceClient : ISkyService, IDisposable
  {
    /// <summary>
    /// Specifies timeout for the whole service call
    /// </summary>
    int TimeoutMs{ get; set;}

    /// <summary>
    /// Indicates that this client instance should take a hold of the underlying data transmission mechanism (such as Glue transport)
    ///  and keep it reserved for subsequent calls. This may reduce latency for cases when many calls need to be executed in order.
    ///  The transport is reserved until this property is either reset to false or client instance is disposed.
    /// For service clients which are based on Glue refer to Azos.Glue.ClientEndPoint.ReserveTransport
    /// </summary>
    bool ReserveTransport{ get; set;}
  }

}
