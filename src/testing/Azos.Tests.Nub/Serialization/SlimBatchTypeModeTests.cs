/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.IO;

using Azos.Scripting;
using Azos.Serialization.Slim;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class SlimBatchTypeModeTests
  {

      [Run]
      public void T1_TypeWasAdded()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          Aver.IsTrue(TypeRegistryMode.PerCall == s1.TypeMode);
          Aver.IsTrue( s1.IsThreadSafe );
          Aver.IsFalse( s1.BatchTypesAdded );

          s1.TypeMode = TypeRegistryMode.Batch;

          Aver.IsTrue(TypeRegistryMode.Batch == s1.TypeMode);
          Aver.IsFalse( s1.IsThreadSafe );
          Aver.IsFalse( s1.BatchTypesAdded );


          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer();
          s2.TypeMode = TypeRegistryMode.Batch;
          var o2 = (A1)s2.Deserialize(ms);

          Aver.AreEqual( 12, o2.I1);

          Aver.IsTrue( s1.BatchTypesAdded );
          Aver.IsTrue( s2.BatchTypesAdded );
        }
      }

       [Run]
      public void T1_ResetCallBatch()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          s1.TypeMode = TypeRegistryMode.Batch;

          Aver.IsTrue(TypeRegistryMode.Batch == s1.TypeMode);
          Aver.IsFalse( s1.IsThreadSafe );
          Aver.IsFalse( s1.BatchTypesAdded );

          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);

          Aver.IsTrue( s1.BatchTypesAdded );

          s1.ResetCallBatch();

          Aver.IsFalse( s1.BatchTypesAdded );
        }
      }

      [Run]
      public void T1_TypeWasNotAdded()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer( new Type[]{typeof(A1)});//put it in globals
          s1.TypeMode = TypeRegistryMode.Batch;
          Aver.IsFalse( s1.BatchTypesAdded );


          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer(new Type[]{typeof(A1)});
          s2.TypeMode = TypeRegistryMode.Batch;
          var o2 = (A1)s2.Deserialize(ms);

          Aver.AreEqual( 12, o2.I1);

          Aver.IsFalse( s1.BatchTypesAdded );
          Aver.IsFalse( s2.BatchTypesAdded );
        }
      }




      [Run]
      [Aver.Throws(typeof(SlimDeserializationException), Message="count mismatch", MsgMatch=Aver.ThrowsAttribute.MatchType.Contains)]
      public void T2_PerCall_CountMismatch()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer(new Type[]{typeof(A1)});
          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer();
          var o2 = (A1)s2.Deserialize(ms);
        }
      }

      [Run]
      [Aver.Throws(typeof(SlimDeserializationException), Message="CSUM mismatch", MsgMatch=Aver.ThrowsAttribute.MatchType.Contains)]
      public void T2_PerCall_CSUM_Mismatch()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer(new Type[]{typeof(A1)});
          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer(new Type[]{typeof(A2)});
          var o2 = (A1)s2.Deserialize(ms);
        }
      }


      [Run]
      [Aver.Throws(typeof(SlimDeserializationException), Message="count mismatch", MsgMatch=Aver.ThrowsAttribute.MatchType.Contains)]
      public void T3_Batch_CountMismatch()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer(new Type[]{typeof(A1)});
          s1.TypeMode= TypeRegistryMode.Batch;
          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer();
          s2.TypeMode= TypeRegistryMode.Batch;
          var o2 = (A1)s2.Deserialize(ms);
        }
      }

      [Run]
      [Aver.Throws(typeof(SlimDeserializationException), Message="CSUM mismatch", MsgMatch=Aver.ThrowsAttribute.MatchType.Contains)]
      public void T3_Batch_CSUM_Mismatch()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer(new Type[]{typeof(A1)});
          s1.TypeMode= TypeRegistryMode.Batch;
          var o1 = new A1{ I1 = 12};

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer(new Type[]{typeof(A2)});
          s2.TypeMode= TypeRegistryMode.Batch;
          var o2 = (A1)s2.Deserialize(ms);
        }
      }


      [Run]
      public void T4_Batch_WriteMany()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          s1.TypeMode= TypeRegistryMode.Batch;
          var o1a = new A1{ I1 = 12};
          var o1b = new A2{ I2 = 13};
          var o1c = new A1{ I1 = 14};
          var o1d = new A1{ I1 = 15};
          var o1e = new A1{ I1 = 16};

          s1.Serialize(ms, o1a);  Aver.IsTrue( s1.BatchTypesAdded );
          s1.Serialize(ms, o1b);  Aver.IsTrue( s1.BatchTypesAdded );
          s1.Serialize(ms, o1c);  Aver.IsFalse( s1.BatchTypesAdded );
          s1.Serialize(ms, o1d);  Aver.IsFalse( s1.BatchTypesAdded );
          s1.Serialize(ms, o1e);  Aver.IsFalse( s1.BatchTypesAdded );
          ms.Seek(0, SeekOrigin.Begin);

          var buf = ms.GetBuffer();
          Console.WriteLine( buf.ToDumpString(DumpFormat.Printable, 0,(int) ms.Length) );

          var s2 = new SlimSerializer();
          s2.TypeMode= TypeRegistryMode.Batch;
          var o2a = (A1)s2.Deserialize(ms); Aver.IsTrue( s2.BatchTypesAdded );
          var o2b = (A2)s2.Deserialize(ms); Aver.IsTrue( s2.BatchTypesAdded );
          var o2c = (A1)s2.Deserialize(ms); Aver.IsFalse( s2.BatchTypesAdded );
          var o2d = (A1)s2.Deserialize(ms); Aver.IsFalse( s2.BatchTypesAdded );
          var o2e = (A1)s2.Deserialize(ms); Aver.IsFalse( s2.BatchTypesAdded );

          Aver.AreEqual(12, o2a.I1);
          Aver.AreEqual(13, o2b.I2);
          Aver.AreEqual(14, o2c.I1);
          Aver.AreEqual(15, o2d.I1);
          Aver.AreEqual(16, o2e.I1);
        }
      }


      [Run]
      public void T5_Batch_WriteReadMany()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          s1.TypeMode= TypeRegistryMode.Batch;
          var s2 = new SlimSerializer();
          s2.TypeMode= TypeRegistryMode.Batch;

          var o1a = new A1{ I1 = 12};
          var o1b = new A2{ I2 = 13};
          var o1c = new A1{ I1 = 14};
          var o1d = new A1{ I1 = 15};
          var o1e = new A1{ I1 = 16};

          s1.Serialize(ms, o1a);  Aver.IsTrue( s1.BatchTypesAdded );
          ms.Position = 0;
          var o2a = (A1)s2.Deserialize(ms); Aver.IsTrue( s2.BatchTypesAdded );

          ms.Position = 0;
          s1.Serialize(ms, o1b);  Aver.IsTrue( s1.BatchTypesAdded );
          ms.Position = 0;
          var o2b = (A2)s2.Deserialize(ms); Aver.IsTrue( s2.BatchTypesAdded );

          ms.Position = 0;
          s1.Serialize(ms, o1c);  Aver.IsFalse( s1.BatchTypesAdded );
          ms.Position = 0;
          var o2c = (A1)s2.Deserialize(ms); Aver.IsFalse( s2.BatchTypesAdded );

          ms.Position = 0;
          s1.Serialize(ms, o1d);  Aver.IsFalse( s1.BatchTypesAdded );
          ms.Position = 0;
          var o2d = (A1)s2.Deserialize(ms); Aver.IsFalse( s2.BatchTypesAdded );

          ms.Position = 0;
          s1.Serialize(ms, o1e);  Aver.IsFalse( s1.BatchTypesAdded );
          ms.Position = 0;
          var o2e = (A1)s2.Deserialize(ms); Aver.IsFalse( s2.BatchTypesAdded );

          Aver.AreEqual(12, o2a.I1);
          Aver.AreEqual(13, o2b.I2);
          Aver.AreEqual(14, o2c.I1);
          Aver.AreEqual(15, o2d.I1);
          Aver.AreEqual(16, o2e.I1);
        }
      }


      [Run]
      [Aver.Throws(typeof(SlimDeserializationException), Message="count mismatch", MsgMatch=Aver.ThrowsAttribute.MatchType.Contains)]
      public void T6_BatchParcelMismatch_WriteMany()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          s1.TypeMode= TypeRegistryMode.Batch;
          var o1a = new A1{ I1 = 12};
          var o1b = new A2{ I2 = 13};
          var o1c = new A1{ I1 = 14};
          var o1d = new A1{ I1 = 15};
          var o1e = new A1{ I1 = 16};

          s1.Serialize(ms, o1a);  Aver.IsTrue( s1.BatchTypesAdded );
          s1.Serialize(ms, o1b);  Aver.IsTrue( s1.BatchTypesAdded );
          s1.Serialize(ms, o1c);  Aver.IsFalse( s1.BatchTypesAdded );
          s1.Serialize(ms, o1d);  Aver.IsFalse( s1.BatchTypesAdded );
          s1.Serialize(ms, o1e);  Aver.IsFalse( s1.BatchTypesAdded );
          ms.Seek(0, SeekOrigin.Begin);

          var buf = ms.GetBuffer();
          Console.WriteLine( buf.ToDumpString(DumpFormat.Printable, 0,(int) ms.Length) );

          var s2 = new SlimSerializer();
          s2.TypeMode= TypeRegistryMode.PerCall;//MISMATCH!!!
          var o2a = (A1)s2.Deserialize(ms);
          var o2b = (A2)s2.Deserialize(ms);//exception

        }
      }


      [Run]
      [Aver.Throws(typeof(SlimDeserializationException), Message="count mismatch", MsgMatch=Aver.ThrowsAttribute.MatchType.Contains)]
      public void T7_CountMismatchResetBatch_WriteMany()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          s1.TypeMode= TypeRegistryMode.Batch;
          var o1a = new A1{ I1 = 12};
          var o1b = new A2{ I2 = 13};
          var o1c = new A1{ I1 = 14};
          var o1d = new A1{ I1 = 15};
          var o1e = new A1{ I1 = 16};

          s1.Serialize(ms, o1a);  Aver.IsTrue( s1.BatchTypesAdded );
          s1.Serialize(ms, o1b);  Aver.IsTrue( s1.BatchTypesAdded );
          s1.Serialize(ms, o1c);  Aver.IsFalse( s1.BatchTypesAdded );
          s1.Serialize(ms, o1d);  Aver.IsFalse( s1.BatchTypesAdded );
          s1.Serialize(ms, o1e);  Aver.IsFalse( s1.BatchTypesAdded );
          ms.Seek(0, SeekOrigin.Begin);

          var buf = ms.GetBuffer();
          Console.WriteLine( buf.ToDumpString(DumpFormat.Printable, 0,(int) ms.Length) );

          var s2 = new SlimSerializer();
          s2.TypeMode= TypeRegistryMode.Batch;
          var o2a = (A1)s2.Deserialize(ms); Aver.IsTrue( s2.BatchTypesAdded );
          Aver.AreEqual(12, o2a.I1);

          s2.ResetCallBatch();
          var o2b = (A2)s2.Deserialize(ms); //Exception
        }
      }



      [Run]
      public void TZ9999_StringInVariousLanguages()
      {
        using (var ms = new MemoryStream())
        {
          var s1 = new SlimSerializer();
          var o1 = new A3{ Text = "Hello 久有归天愿,Աեցեհի, Не менее 100 советских самолетов поднялись в воздух, asağıda yağız yer yaratıldıkta. I Agree!"};

          Console.WriteLine(o1.Text);

          s1.Serialize(ms, o1);
          ms.Seek(0, SeekOrigin.Begin);

          var s2 = new SlimSerializer();
          var o2 = (A3)s2.Deserialize(ms);

          Console.WriteLine(o2.Text);

          Aver.AreEqual(o1.Text, o2.Text);
        }
      }




      public class A1
      {
        public int I1;
      }

      public class A2
      {
        public int I2;
      }

      public class A3
      {
        public string Text;
      }


  }

}
