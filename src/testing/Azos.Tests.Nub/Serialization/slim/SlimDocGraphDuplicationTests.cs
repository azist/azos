/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.IO;

using Azos.Scripting;
using Azos.IO;
using Azos.Serialization.Slim;
using System.Collections.Generic;
using Azos.Data;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class SlimDocGraphDuplicationTests //Issue #283
  {

    public class ObjectA : TypedDoc
    {
      public List<ObjectB> Values { get; set; }
    }

    public class ObjectB : TypedDoc  //it only fails WHEN this is inherited from TypedDoc
    {
      public int BField;
    }


    [Run]
    public void T01()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer(SlimFormat.Instance);

        var root = new ObjectA()
        {
          Values = new List<ObjectB>{   //error shows when more than 3 items is added
          new ObjectB{ BField =10000000},
          new ObjectB{ BField =21},
          new ObjectB{ BField =31},
          new ObjectB{ BField =41},
          new ObjectB{ BField =51},
          new ObjectB{ BField =61},
          new ObjectB{ BField =71},
          new ObjectB{ BField =81},
          new ObjectB{ BField =91},
          new ObjectB{ BField =101}
         }
        };

        s.Serialize(ms, root);

        ms.Position = 0;

        var deser = s.Deserialize(ms) as ObjectA;

        Aver.IsNotNull(deser);
        Aver.AreEqual(root.Values.Count, deser.Values.Count);
        deser.See();
      }
    }

  }
}
