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

using Azos.DataAccess.Distributed;
using Azos.IO;
using Azos.Scripting;
using Azos.Serialization.Arow;

namespace Azos.Tests.Unit.Serialization
{
    [Runnable]
    public class ARowBenchmarking : IRunnableHook
    {
      void IRunnableHook.Prologue(Runner runner, FID id)
      {
        ArowSerializer.RegisterTypeSerializationCores( Assembly.GetExecutingAssembly() );
      }

      bool IRunnableHook.Epilogue(Runner runner, FID id, Exception error) => false;


      [Run]
      public void Serialize_SimplePerson_Arow()
      {
        const int CNT = 250000;

        var row = getSimplePerson();

        var streamer = SlimFormat.Instance.GetWritingStreamer();

        using(var ms = new MemoryStream())
        {
          var sw = Stopwatch.StartNew();
          for(var i=0; i<CNT; i++)
          {
            ms.Position = 0;
            streamer.BindStream(ms);
            ArowSerializer.Serialize(row, streamer);
            streamer.UnbindStream();
          }

          var el = sw.ElapsedMilliseconds;
          Console.WriteLine("Arow did {0:n0} in {1:n0} ms at {2:n0} ops/sec. Stream Size is: {3:n0} bytes".Args( CNT, el, CNT / (el/1000d), ms.Length ));
        }
      }

      [Run]
      public void Deserialize_SimplePerson_Arow()
      {
        const int CNT = 250000;

        var row = getSimplePerson();

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();

        using(var ms = new MemoryStream())
        {
          writer.BindStream(ms);
          ArowSerializer.Serialize(row, writer);
          writer.UnbindStream();

          reader.BindStream(ms);

          var sw = Stopwatch.StartNew();
          for(var i=0; i<CNT; i++)
          {
            ms.Position = 0;
            var row2 = new SimplePersonRow();
            ArowSerializer.Deserialize(row2, reader);
            Aver.AreEqual(row.ID, row2.ID);
          }

          var el = sw.ElapsedMilliseconds;
          Console.WriteLine("Arow did {0:n0} in {1:n0} ms at {2:n0} ops/sec. Stream Size is: {3:n0} bytes".Args( CNT, el, CNT / (el/1000d), ms.Length ));
        }
      }

      [Run]
      public void Serialize_SimplePerson_Slim()
      {
        const int CNT = 250000;

        var row = getSimplePerson();

        var slim = new Azos.Serialization.Slim.SlimSerializer( Azos.Serialization.Slim.TypeRegistry.BoxedCommonNullableTypes,
                                                              Azos.Serialization.Slim.TypeRegistry.BoxedCommonTypes,
                                                              new []{ typeof(SimplePersonRow) });

        slim.TypeMode = Azos.Serialization.Slim.TypeRegistryMode.Batch;//give slim all possible preferences

        using(var ms = new MemoryStream())
        {
          slim.Serialize(ms, row);//warmup
          var sw = Stopwatch.StartNew();
          for(var i=0; i<CNT; i++)
          {
            ms.Position = 0;
            slim.Serialize(ms, row);
          }

          var el = sw.ElapsedMilliseconds;
          Console.WriteLine("Slim did {0:n0} in {1:n0} ms at {2:n0} ops/sec. Stream Size is: {3:n0} bytes".Args( CNT, el, CNT / (el/1000d), ms.Length ));
        }
      }


      [Run]
      public void Deserialize_SimplePerson_Slim()
      {
        const int CNT = 250000;

        var row = getSimplePerson();

        var slim = new Azos.Serialization.Slim.SlimSerializer( Azos.Serialization.Slim.TypeRegistry.BoxedCommonNullableTypes,
                                                              Azos.Serialization.Slim.TypeRegistry.BoxedCommonTypes,
                                                              new []{ typeof(SimplePersonRow) });

        slim.TypeMode = Azos.Serialization.Slim.TypeRegistryMode.Batch;//give slim all possible preferences

        using(var ms = new MemoryStream())
        {
          slim.Serialize(ms, row);//warmup
          var sw = Stopwatch.StartNew();
          for(var i=0; i<CNT; i++)
          {
            ms.Position = 0;
            var row2 = slim.Deserialize(ms) as SimplePersonRow;
            Aver.AreEqual(row.ID, row2.ID);
          }

          var el = sw.ElapsedMilliseconds;
          Console.WriteLine("Slim did {0:n0} in {1:n0} ms at {2:n0} ops/sec. Stream Size is: {3:n0} bytes".Args( CNT, el, CNT / (el/1000d), ms.Length ));
        }
      }

      [Run]
      public void Serialize_Family_Arow()
      {
        const int CNT = 250000;

        var row = getFamily();

        var streamer = SlimFormat.Instance.GetWritingStreamer();

        using(var ms = new MemoryStream())
        {
          var sw = Stopwatch.StartNew();
          for(var i=0; i<CNT; i++)
          {
            ms.Position = 0;
            streamer.BindStream(ms);
            ArowSerializer.Serialize(row, streamer);
            streamer.UnbindStream();
          }

          var el = sw.ElapsedMilliseconds;
          Console.WriteLine("Arow did {0:n0} in {1:n0} ms at {2:n0} ops/sec. Stream Size is: {3:n0} bytes".Args( CNT, el, CNT / (el/1000d), ms.Length ));
        }
      }

      [Run]
      public void Deserialize_Family_Arow()
      {
        const int CNT = 250000;

        var row = getFamily();

        var writer = SlimFormat.Instance.GetWritingStreamer();
        var reader = SlimFormat.Instance.GetReadingStreamer();

        using(var ms = new MemoryStream())
        {
          writer.BindStream(ms);
          ArowSerializer.Serialize(row, writer);
          writer.UnbindStream();
          reader.BindStream(ms);

          var sw = Stopwatch.StartNew();
          for(var i=0; i<CNT; i++)
          {
            ms.Position = 0;
            var row2 = new FamilyRow();
            ArowSerializer.Deserialize(row2, reader);
            Aver.AreEqual(row.ID, row2.ID);
          }

          var el = sw.ElapsedMilliseconds;
          Console.WriteLine("Arow did {0:n0} in {1:n0} ms at {2:n0} ops/sec. Stream Size is: {3:n0} bytes".Args( CNT, el, CNT / (el/1000d), ms.Length ));
        }
      }

      [Run]
      public void Serialize_Family_Slim()
      {
        const int CNT = 250000;

        var row = getFamily();

        var slim = new Azos.Serialization.Slim.SlimSerializer( Azos.Serialization.Slim.TypeRegistry.BoxedCommonNullableTypes,
                                                              Azos.Serialization.Slim.TypeRegistry.BoxedCommonTypes,
                                                              new []{ typeof(SimplePersonRow),typeof(SimplePersonRow[]), typeof(List<SimplePersonRow>), typeof(FamilyRow) });

        slim.TypeMode = Azos.Serialization.Slim.TypeRegistryMode.Batch;//give slim all possible preferences

        using(var ms = new MemoryStream())
        {
          slim.Serialize(ms, row);//warmup
          var sw = Stopwatch.StartNew();
          for(var i=0; i<CNT; i++)
          {
            ms.Position = 0;
            slim.Serialize(ms, row);
          }

          var el = sw.ElapsedMilliseconds;
          Console.WriteLine("Slim did {0:n0} in {1:n0} ms at {2:n0} ops/sec. Stream Size is: {3:n0} bytes".Args( CNT, el, CNT / (el/1000d), ms.Length ));
        }
      }


      [Run]
      public void Deserialize_Family_Slim()
      {
        const int CNT = 250000;

        var row = getFamily();

        var slim = new Azos.Serialization.Slim.SlimSerializer( Azos.Serialization.Slim.TypeRegistry.BoxedCommonNullableTypes,
                                                              Azos.Serialization.Slim.TypeRegistry.BoxedCommonTypes,
                                                              new []{ typeof(SimplePersonRow),typeof(SimplePersonRow[]), typeof(List<SimplePersonRow>), typeof(FamilyRow) });

        slim.TypeMode = Azos.Serialization.Slim.TypeRegistryMode.Batch;//give slim all possible preferences

        using(var ms = new MemoryStream())
        {
          slim.Serialize(ms, row);//warmup
          var sw = Stopwatch.StartNew();
          for(var i=0; i<CNT; i++)
          {
            ms.Position = 0;
            var row2 = slim.Deserialize(ms) as FamilyRow;
            Aver.AreEqual(row.ID, row2.ID);
          }

          var el = sw.ElapsedMilliseconds;
          Console.WriteLine("Slim did {0:n0} in {1:n0} ms at {2:n0} ops/sec. Stream Size is: {3:n0} bytes".Args( CNT, el, CNT / (el/1000d), ms.Length ));
        }
      }

    #region .pvt
      private SimplePersonRow getSimplePerson()
      {
        return new SimplePersonRow
        {
           Age = 123, Bool1 =true, ID = new GDID(12,234), Name = "Jacques Jabakz", Salary=143098, Str1="Tryten"
        };
      }

      private FamilyRow getFamily()
      {
        return new FamilyRow
        {
           ID = new GDID(12,3214232333),
           Name = "The Familians",
           Advisers = new List<SimplePersonRow>{ new SimplePersonRow{ Name="Jabitzkiy"}, new SimplePersonRow{ Name="Fixman", Age=4827384}},
           Father = getSimplePerson(),
           Mother = getSimplePerson(),
           Brothers = new []{ getSimplePerson() },
           Sisters = new []{ getSimplePerson(), getSimplePerson(), getSimplePerson() },
        };
      }
    #endregion


  }
}
