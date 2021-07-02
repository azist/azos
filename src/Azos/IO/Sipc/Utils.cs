/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;

namespace Azos.IO.Sipc
{
  /// <summary>
  /// Provides low-level utilities for Simple IPC
  /// </summary>
  internal static class Utils
  {
    public const int MAX_BYTE_SZ = 2 * 1024 * 1024;

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
