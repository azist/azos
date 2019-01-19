/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Azos.Scripting;

using Azos.Apps;
using Azos.Data;
using Azos.Conf;
using Azos.Collections;
using Azos.IO;
using Azos.Serialization.Slim;

namespace Azos.Tests.Unit.Serialization
{
  [Runnable(TRUN.BASE)]
  public class Slim
  {

    [Run]
    public void WaveSession()
    {
      using(var ms = new MemoryStream())
      {
        var session = new Azos.Wave.WaveSession(Guid.NewGuid(), 1234567);

        var s = new SlimSerializer(SlimFormat.Instance);

        s.Serialize(ms, session);
        ms.Seek(0, SeekOrigin.Begin);

        var session2 = s.Deserialize(ms) as Azos.Wave.WaveSession;

        Aver.AreEqual(session.ID, session2.ID);
        Aver.AreEqual(session.IDSecret, session2.IDSecret);
        Aver.AreEqual(0, session2.Items.Count);
      }
    }

  }
}
