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
using System.Diagnostics;
using System.Reflection;

using Azos.Scripting;

using Azos.IO;
using Azos.Serialization.Arow;
using Azos.Serialization.JSON;
using Azos.Apps;
using Azos.Financial;
using Azos.DataAccess.Distributed;
using Azos.Apps.Pile;

namespace Azos.Tests.Unit.Serialization
{
    [Runnable(TRUN.BASE)]
    public class ARowAmorphous : IRunnableHook
    {
      void IRunnableHook.Prologue(Runner runner, FID id)
      {
        ArowSerializer.RegisterTypeSerializationCores( Assembly.GetExecutingAssembly() );
      }

      bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error) => false;


      [Run]
      public void ReadIntoSame()
      {
        var v1r1 = new Ver1Row
        {
           A = "A string",
           B = 156,
           C = null,
           D = true,
           E = new byte[]{1,5,98},
           F = new DateTime(1980, 02, 12),
           G = new List<Ver1Row>{ new Ver1Row{ C = -998, D = null }}
        };

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
          writer.BindStream(ms);
          ArowSerializer.Serialize(v1r1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var v1r2 = new Ver1Row();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(v1r2, reader);

          Aver.AreEqual("A string", v1r2.A);
          Aver.AreEqual(156, v1r2.B);
          Aver.IsNull( v1r2.C );
          Aver.IsTrue( v1r2.D.Value );
          Aver.AreEqual(3,  v1r2.E.Length );
          Aver.AreEqual(98,  v1r2.E[2] );
          Aver.AreEqual(new DateTime(1980, 02, 12),  v1r2.F );
          Aver.IsNotNull(v1r2.G);
          Aver.AreEqual(1,  v1r2.G.Count );
          Aver.AreEqual(-998,  v1r2.G[0].C );
          Aver.IsNull( v1r2.G[0].D );
        }
      }

      [Run]
      public void ReadIntoAnother()
      {
        var v1r1 = new Ver1Row
        {
           A = "A string",
           B = 156,
           C = null,
           D = true,
           E = new byte[]{1,5,98},
           F = new DateTime(1980, 02, 12),
           G = new List<Ver1Row>{ new Ver1Row{ C = -998, D = null }}
        };

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();
        using(var ms = new MemoryStream())
        {
          writer.BindStream(ms);
          ArowSerializer.Serialize(v1r1, writer);
          writer.UnbindStream();

          ms.Position = 0;

          var v2r2 = new Ver2Row();
          reader.BindStream(ms);
          ArowSerializer.Deserialize(v2r2, reader);

          Console.WriteLine(v2r2.ToJSON(JSONWritingOptions.PrettyPrintRowsAsMap));

          Aver.AreEqual("A string", v2r2.A);

          Aver.AreEqual(156, v2r2.AmorphousData["b"].AsInt());
          Aver.IsNull(v2r2.AmorphousData["c"]);
          Aver.IsTrue(v2r2.AmorphousData["e"] is byte[]);
          Aver.IsNotNull(v2r2.AmorphousData["g"]);
          Aver.IsTrue(v2r2.AmorphousData["g"] is Array);
          Aver.AreEqual(-998, ((JSONDataMap)((object[])v2r2.AmorphousData["g"])[0])["c"].AsInt());

          Aver.AreEqual(0, v2r2.B);
          Aver.AreEqual(null, v2r2.C);
          Aver.AreEqual(true, v2r2.D);
          Aver.IsNull(v2r2.E);
          Aver.AreEqual(new DateTime(1980, 02, 12), v2r2.F);
          Aver.IsNull(v2r2.G);
        }
      }


    }
}
