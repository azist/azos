/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Erlang
{
  /// <summary>
  /// Factory of IErlTransport
  /// </summary>
  public static class ErlTransportFactory
  {
    #region Public

    public static IErlTransport Create(string transportTypeFullName, string remoteNodeName)
    {
      var res =
        FactoryUtils.Make<IErlTransport>(
          string.Format("nfx{{type=\"{0}\"}}", transportTypeFullName).AsLaconicConfig(),
          typeof (ErlTcpTransport));
      res.NodeName = remoteNodeName;

      return res;
    }

    public static IErlTransport Create(string transportTypeFullName, string remoteNodeName,
                                       string host, int port)
    {
      var res = Create(transportTypeFullName, remoteNodeName);
      res.Connect(host, port);

      return res;
    }

    #endregion
  }
}