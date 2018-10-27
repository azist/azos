
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