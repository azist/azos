/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Azos.IO.Sipc
{
  /// <summary>
  /// Provides Simple IPC protocol definitions
  /// </summary>
  internal static class Protocol
  {
    public const int LISTENER_PORT_DEFAULT = 49123;
    public const int LISTENER_PORT_RANGE_DEFAULT = 5;
    public const int LISTENER_PORT_MIN = 40_000;
    public const int LISTENER_PORT_MAX = 49_150;


    public const int RECEIVE_TIMEOUT_MS = 5000;
    public const int SEND_TIMEOUT_MS = 5000;

    public const int RECEIVE_BUFFER_SIZE = 8 * 1024;
    public const int SEND_BUFFER_SIZE = 8 * 1024;

    public const int MAX_BYTE_SZ = 1 * 1024 * 1024;


    public static int GuardPort(this int port, string name)
     => port.IsTrue(v => v >= LISTENER_PORT_MIN && v <= LISTENER_PORT_MAX, "{2} >= (`{0}`={1}) <= {3}".Args(name, port, LISTENER_PORT_MIN, LISTENER_PORT_MAX));

    public static void Send(TcpClient client, string data)
    {
      client.NonNull(nameof(client));
      data.NonBlank(nameof(data));

      using (var ms = new MemoryStream())
      {
        ms.Position = sizeof(int);  //skip 4 bytes

        using (var wri = new StreamWriter(ms, Encoding.UTF8))
        {
          wri.Write(data);
          wri.Flush();
        }

        var sz = (int)ms.Position;//with 4 bytes size preamble

        (sz < MAX_BYTE_SZ).IsTrue("sz < {0}".Args(MAX_BYTE_SZ));

        ms.Position = 0;
        ms.WriteBEInt32(sz);//write size

        client.GetStream().Write(ms.GetBuffer(), 0, sz);
      }
    }

    public static string Receive(TcpClient client)
    {
      var nets = client.NonNull(nameof(client)).GetStream();

      using(var ms = new MemoryStream())
      {
        ms.Position = sizeof(int);
        socketRead(nets, ms.GetBuffer(), 0, 4);

        ms.Position = 0;
        var sz = ms.ReadBEInt32();

        (sz < MAX_BYTE_SZ).IsTrue("sz < {0}".Args(MAX_BYTE_SZ));

        ms.SetLength(sz);

        socketRead(nets, ms.GetBuffer(), 0, sz - sizeof(int));

        using(var reader = new StreamReader(ms, Encoding.UTF8))
          return reader.ReadToEnd();
      }
    }

    //return connection id/name
    public static string ReceiveHandshake(TcpClient client)
    {
      const int GUID_SZ = 16;

      var nets = client.NonNull(nameof(client)).GetStream();

      var buf = new byte[GUID_SZ];
      socketRead(nets, buf, 0, GUID_SZ);

      return buf.GuidFromNetworkByteOrder().ToString();
    }

    private static void socketRead(NetworkStream nets, byte[] buffer, int offset, int total)
    {
      int cnt = 0;
      while (cnt < total)
      {
        var got = nets.Read(buffer, offset + cnt, total - cnt);
        if (got <= 0) throw new SocketException();//torn
        cnt += got;
      }
    }

  }
}
