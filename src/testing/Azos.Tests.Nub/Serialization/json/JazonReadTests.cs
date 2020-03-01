using System;
using System.Collections.Generic;
using System.Text;

using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.Serialization.JSON.Backends;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JazonReadTests
  {
    private static readonly IJsonReaderBackend JZ = new JazonReaderBackend();

    [Run]
    public void Doubles()
    {
      var map = JZ.DeserializeFromJson("{ pi: 3.14159265359, exp1: 123e4, exp2: 2e-5, exp3: 2e+3, exp4: -0.2e+2 }", true) as JsonDataMap;
      Aver.AreEqual(5, map.Count);
      Aver.AreObjectsEqual(3.14159265359D, map["pi"]);
      Aver.AreObjectsEqual(123e4D, map["exp1"]);
      Aver.AreObjectsEqual(2e-5D, map["exp2"]);
      Aver.AreObjectsEqual(2e+3D, map["exp3"]);
      Aver.AreObjectsEqual(-0.2e+2D, map["exp4"]);
    }

  }
}
