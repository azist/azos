/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Runtime.Serialization;
using System.IO;

using Azos.Scripting;
using Azos.Serialization.Slim;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class Slim4Tests
  {

    [Run]
    public void T01()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new customClassA
        {
          A = 190,
          B = 3232,
          Child = null
        };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (customClassB)s.Deserialize(ms);

        Aver.AreEqual( 190, dOut.A );
        Aver.AreEqual( 3232, dOut.B );
        Aver.IsNull( dOut.Child );
      }
    }

    [Run]
    public void T02()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new customClassA
        {
          A = 2190,
          B = 23232,
          Child = new customClassA{ A = -100, B=-900}
        };

        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (customClassB)s.Deserialize(ms);

        Aver.AreEqual( 2190, dOut.A );
        Aver.AreEqual( 23232, dOut.B );
        Aver.IsNotNull( dOut.Child );
        Aver.AreEqual( -100, dOut.Child.A );
        Aver.AreEqual( -900, dOut.Child.B );
      }
    }


    [Run]
    public void T03()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new[]{
         new customClassB {  A = 1, B = 100},
         new customClassB {  A = 2, B = 200},
         new customClassB {  A = 3, B = 300},
        };

        //////StackOverflow while using ISerializable
        ////dIn[0].Child = dIn[2];
        ////dIn[1].Child = dIn[0];
        ////dIn[2].Child = dIn[1];


        s.Serialize(ms, dIn);
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (customClassB[])s.Deserialize(ms);

        Aver.AreEqual( 3, dOut.Length );
        Aver.AreEqual( 1, dOut[0].A);
        Aver.AreEqual( 100, dOut[0].B);

      }
    }

    [Run("!slim-stack","")]
    public void T04()
    {
      using (var ms = new MemoryStream())
      {
        var s = new SlimSerializer();

        var dIn = new[]{
         new customClassA {  A = 1, B = 100},
         new customClassA {  A = 2, B = 200},
         new customClassA {  A = 3, B = 300},
        };

        //StackOverflow while using ISerializable
        dIn[0].Child = dIn[2];
        dIn[1].Child = dIn[0];
        dIn[2].Child = dIn[1];


        s.Serialize(ms, dIn);
        Console.WriteLine("Written {0} bytes".Args(ms.Length));
        ms.Seek(0, SeekOrigin.Begin);

        var dOut = (customClassB[])s.Deserialize(ms);

        Aver.AreEqual( 3, dOut.Length );
        Aver.AreEqual( 1, dOut[0].A);
        Aver.AreEqual( 100, dOut[0].B);

      }
    }



    private class customClassA : ISerializable
    {
      protected int m_A;
      protected int m_B;
      protected customClassA m_Child;

      public int A{ get{ return m_A; } set{ m_A = value; }}
      public int B{ get{ return m_B; } set{ m_B = value; }}
      public customClassA Child{ get{ return m_Child;} set{ m_Child = value; }}

      public void GetObjectData(SerializationInfo info, StreamingContext context)
      {
        info.SetType(typeof(customClassB));//<-----------------------------------------
        info.AddValue("a", m_A);
        info.AddValue("b", m_B);
        info.AddValue("child", m_Child);
      }
    }

    private class customClassB : customClassA
    {
      public customClassB(){}
      public customClassB(SerializationInfo info, StreamingContext context)
      {
        m_A = info.GetInt32("a");
        m_B = info.GetInt32("b");
        m_Child = info.GetValue("child", typeof(customClassA)) as customClassA;
      }
    }


  }
}
