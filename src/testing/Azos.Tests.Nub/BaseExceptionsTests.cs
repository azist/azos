/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

using Azos.Scripting;

using Azos.Serialization.JSON;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class BaseExceptionsTests
  {

    [Run]
    public void WrappedExceptionData_JSON()
    {
      try
      {
        throw new AzosException("Oy vei!", new AzosException("Inside")){ Code = 223322, Source = "Karlson" };
      }
      catch(Exception caught)
      {
        var wed = new WrappedExceptionData(caught);

        var json = wed.ToJson(JsonWritingOptions.CompactRowsAsMap);

        json.See();

        var wed2 = JsonReader.ToDoc<WrappedExceptionData>(json);

        averWrappedExceptionEquality(wed, wed2);
      }
    }

    [Run]
    public void WrappedExceptionData_Slim()
    {
      try
      {
        throw new AzosException("Oy vei!", new AzosException("Inside")){ Code = 223322, Source = "Karlson" };
      }
      catch(Exception caught)
      {
        var wed = new WrappedExceptionData(caught);
        var ser = new Azos.Serialization.Slim.SlimSerializer();

        using(var ms = new MemoryStream())
        {
          ser.Serialize(ms, wed);
          ms.Position = 0;

          var bin = ms.ToArray();
          Console.WriteLine();
          Console.WriteLine($"Bin {bin.Length} bytes:");
          Console.WriteLine($"-----------------------------");
          Console.WriteLine(bin.ToDumpString(DumpFormat.Hex));

          var wed2 = ser.Deserialize(ms) as WrappedExceptionData;
          averWrappedExceptionEquality(wed, wed2);
        }
      }
    }


    private void averWrappedExceptionEquality(WrappedExceptionData d1, WrappedExceptionData d2)
    {
      Aver.IsNotNull(d1);
      Aver.IsNotNull(d2);
      Aver.AreNotSameRef(d1, d2);

      Aver.AreEqual(d1.Message, d2.Message);
      Aver.AreEqual(d1.Code, d2.Code);
      Aver.AreEqual(d1.Source, d2.Source);
      Aver.AreEqual(d1.TypeName, d2.TypeName);
      Aver.AreEqual(d1.AppName, d2.AppName);
      Aver.AreEqual(d1.AppId, d2.AppId);
      Aver.AreEqual(d1.StackTrace, d2.StackTrace);
      Aver.AreEqual(d1.ExternalStatus.ToJson(), d2.ExternalStatus.ToJson());

      if (d1.InnerException!=null)
        averWrappedExceptionEquality(d1.InnerException, d2.InnerException);
    }

  }
}
