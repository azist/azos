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
    public void WrappedExceptionData_BSON()
    {
      try
      {
        throw new AzosException("Oy vei!", new AzosException("Inside")){ Code = 223322, Source = "Karlson" };
      }
      catch(Exception caught)
      {
        var wed = new WrappedExceptionData(caught);
        var ser = new Azos.Serialization.BSON.BSONSerializer();

        var doc = ser.Serialize(wed);

        var wed2 = new WrappedExceptionData();
        object ctx = null;
        wed2.DeserializeFromBSON(ser, doc, ref ctx);

        Console.WriteLine();
        Console.WriteLine($"BSON:");
        Console.WriteLine($"-----------------------------");
        Console.WriteLine(doc.ToJson());

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


    [Run]
    public void WrappedExceptionData_BASE64()
    {
      try
      {
        throw new AzosException("Oy vei!", new AzosException("Inside")){ Code = 223322, Source = "Karlson" };
      }
      catch(Exception caught)
      {
        var wed = new WrappedExceptionData(caught);
        var base64 = wed.ToBase64();

        Console.WriteLine();
        Console.WriteLine($"Base64 {base64.Length} bytes:");
        Console.WriteLine($"-----------------------------");
        Console.WriteLine(base64);

        var wed2 = WrappedExceptionData.FromBase64(base64);
        averWrappedExceptionEquality(wed, wed2);
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
      Aver.AreEqual(d1.ApplicationName, d2.ApplicationName);
      Aver.AreEqual(d1.StackTrace, d2.StackTrace);
      Aver.AreEqual(d1.WrappedData, d2.WrappedData);

      if (d1.InnerException!=null)
        averWrappedExceptionEquality(d1.InnerException, d2.InnerException);
    }

  }
}
