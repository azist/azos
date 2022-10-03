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


    public const int RECEIVE_TIMEOUT_MS = 2000;
    public const int SEND_TIMEOUT_MS = 2000;

    public const int RECEIVE_BUFFER_SIZE = 8 * 1024;
    public const int SEND_BUFFER_SIZE = 8 * 1024;

    public const int MAX_BYTE_SZ = 1 * 1024 * 1024;

    public const int PING_INTERVAL_MS = 2500;
    public const int LIMBO_TIMEOUT_MS = PING_INTERVAL_MS * 4;//if more than 4 ping interval passes, connection is in limbo

    public const string CMD_PING = "ping;";
    public const string CMD_STOP = "stop;";
    public const string CMD_GC = "gc;";
    public const string CMD_DISCONNECT = "disconnect;";


    public static int GuardPort(this int port, string name)
     => port.IsTrue(v => v >= LISTENER_PORT_MIN && v <= LISTENER_PORT_MAX, "{2} >= (`{0}`={1}) <= {3}".Args(name, port, LISTENER_PORT_MIN, LISTENER_PORT_MAX));

    private const int DEFAULT_STREAM_CAPACITY = 32 * 1024;

    public static void Send(TcpClient client, string data)
    {
      client.NonNull(nameof(client));
      data.NonBlank(nameof(data));

      using (var ms = new MemoryStream(DEFAULT_STREAM_CAPACITY))
      {
        ms.Position = sizeof(int);  //skip 4 bytes

        using (var wri = new StreamWriter(ms, Encoding.UTF8, 8 * 1024, true))
        {
          wri.Write(data);
          wri.Flush();
        }

        var sz = (int)ms.Length - sizeof(int);//just the size of string w/o first 4 bytes

        (sz < MAX_BYTE_SZ).IsTrue("sz < {0}".Args(MAX_BYTE_SZ));

        ms.Position = 0;
        ms.WriteBEInt32(sz);//write byte size of data
        client.GetStream().Write(ms.GetBuffer(), 0, (int)ms.Length);
      }
    }

    public static string Receive(TcpClient client)
    {
      var nets = client.NonNull(nameof(client)).GetStream();

      using(var ms = new MemoryStream(DEFAULT_STREAM_CAPACITY))
      {
        ms.SetLength(sizeof(int));
        socketRead(nets, ms.GetBuffer(), 0, sizeof(int));

        ms.Position = 0;
        var sz = ms.ReadBEInt32();
        (sz < MAX_BYTE_SZ).IsTrue("sz < {0}".Args(MAX_BYTE_SZ));

        ms.SetLength(sz);

        socketRead(nets, ms.GetBuffer(), 0, sz);

        ms.Position = 0;
        using(var reader = new StreamReader(ms, Encoding.UTF8))
          return reader.ReadToEnd();
      }
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
