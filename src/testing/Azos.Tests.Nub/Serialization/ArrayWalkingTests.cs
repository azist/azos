/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class ArrayWalkingTests
  {
    [Run]
    public void Utils_WalkArrayWrite_1D()
    {
      var arr1 = new object[25];

      var cnt = 0;
      Azos.Serialization.SerializationUtils.WalkArrayWrite(arr1, elm => cnt++);

      Aver.AreEqual(arr1.Length, cnt);
    }

    [Run]
    public void Utils_WalkArrayRead_1D()
    {
      var arr1 = new object[25];

      var cnt = 0;
      Azos.Serialization.SerializationUtils.WalkArrayRead(arr1, () => cnt++);

      Aver.AreEqual(arr1.Length, cnt);
    }

    [Run]
    public void Utils_WalkArrayWrite_2D_1()
    {
      var arr1 = new object[2, 25];

      var cnt = 0;
      Azos.Serialization.SerializationUtils.WalkArrayWrite(arr1, elm => cnt++);

      Aver.AreEqual(50, cnt);
      Aver.AreEqual(arr1.Length, cnt);
    }

    [Run]
    public void Utils_WalkArrayRead_2D_1()
    {
      var arr1 = new object[2, 25];

      var cnt = 0;
      Azos.Serialization.SerializationUtils.WalkArrayRead(arr1, () => cnt++);

      Aver.AreEqual(50, cnt);
      Aver.AreEqual(arr1.Length, cnt);
    }

    [Run]
    public void Utils_WalkArrayWrite_2D_2()
    {
      var arr1 = new object[25, 2];

      var cnt = 0;
      Azos.Serialization.SerializationUtils.WalkArrayWrite(arr1, elm => cnt++);

      Aver.AreEqual(50, cnt);
      Aver.AreEqual(arr1.Length, cnt);
    }

    [Run]
    public void Utils_WalkArrayRead_2D_2()
    {
      var arr1 = new object[25, 2];

      var cnt = 0;
      Azos.Serialization.SerializationUtils.WalkArrayRead(arr1, () => cnt++);

      Aver.AreEqual(50, cnt);
      Aver.AreEqual(arr1.Length, cnt);
    }

    [Run]
    public void Utils_WalkArrayWrite_3D()
    {
      var arr1 = new object[8, 2, 4];

      var cnt = 0;
      Azos.Serialization.SerializationUtils.WalkArrayWrite(arr1, elm => cnt++);

      Aver.AreEqual(arr1.Length, cnt);
      Aver.AreEqual(64, cnt);
    }

    [Run]
    public void Utils_WalkArrayRead_3D()
    {
      var arr1 = new object[8, 2, 4];
      var cnt = 0;
      Azos.Serialization.SerializationUtils.WalkArrayRead<object>(arr1, () => cnt++);

      Aver.AreEqual(arr1.Length, cnt);
      Aver.AreEqual(64, cnt);
    }

    [Run]
    public void Utils_WalkArrayWrite_4D()
    {
      var arr1 = new object[8, 2, 4, 10];

      var cnt = 0;
      Azos.Serialization.SerializationUtils.WalkArrayWrite(arr1, elm => cnt++);

      Aver.AreEqual(arr1.Length, cnt);
      Aver.AreEqual(640, cnt);
    }

    [Run]
    public void Utils_WalkArrayRead_4D()
    {
      var arr1 = new object[8, 2, 4, 10];

      var cnt = 0;
      Azos.Serialization.SerializationUtils.WalkArrayRead<object>(arr1, () => cnt++);

      Aver.AreEqual(arr1.Length, cnt);
      Aver.AreEqual(640, cnt);
    }

  }
}
