/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Linq;
using Azos.Scripting;
using Azos.Text;

namespace Azos.Tests.Nub.Parsing
{
  [Runnable]
  public class TagParsingTests
  {

    [Run]
    public void ParseSegments_00()
    {
      var got = "".ParseSegments().ToArray();
      Aver.AreEqual(0, got.Length);

      got = ((string)null).ParseSegments().ToArray();
      Aver.AreEqual(0, got.Length);

      got = "   ".ParseSegments().ToArray();
      Aver.AreEqual(1, got.Length);
      Aver.IsFalse(got[0].IsTag);
      Aver.AreEqual("   ", got[0].Content);
      Aver.AreEqual(0, got[0].IdxStart);
      Aver.AreEqual(2, got[0].IdxEnd);
      Aver.AreEqual(3, got[0].Length);
    }


    [Run]
    public void ParseSegments_01()
    {
      var got = "How is <monster> doing?".ParseSegments().ToArray();

      got.See();

      Aver.AreEqual(3, got.Length);

      Aver.IsFalse(got[0].IsTag);
      Aver.AreEqual("How is ", got[0].Content);
      Aver.AreEqual(0, got[0].IdxStart);
      Aver.AreEqual(6, got[0].IdxEnd);
      Aver.AreEqual(7, got[0].Length);

      Aver.IsTrue(got[1].IsTag);
      Aver.AreEqual("monster", got[1].Content);
      Aver.AreEqual(7, got[1].IdxStart);
      Aver.AreEqual(15, got[1].IdxEnd);
      Aver.AreEqual(9, got[1].Length);//with < >

      Aver.IsFalse(got[2].IsTag);
      Aver.AreEqual(" doing?", got[2].Content);
      Aver.AreEqual(16, got[2].IdxStart);
      Aver.AreEqual(22, got[2].IdxEnd);
      Aver.AreEqual(7, got[2].Length);
    }


    [Run]
    public void ParseSegments_02()
    {
      var got = "How is <monster>".ParseSegments().ToArray();

      got.See();

      Aver.AreEqual(2, got.Length);

      Aver.IsFalse(got[0].IsTag);
      Aver.AreEqual("How is ", got[0].Content);
      Aver.AreEqual(0, got[0].IdxStart);
      Aver.AreEqual(6, got[0].IdxEnd);
      Aver.AreEqual(7, got[0].Length);

      Aver.IsTrue(got[1].IsTag);
      Aver.AreEqual("monster", got[1].Content);
      Aver.AreEqual(7, got[1].IdxStart);
      Aver.AreEqual(15, got[1].IdxEnd);
      Aver.AreEqual(9, got[1].Length);//with < >
    }

    [Run]
    public void ParseSegments_02_2()
    {
      var got = "How is <monster".ParseSegments().ToArray();

      got.See();

      Aver.AreEqual(2, got.Length);

      Aver.IsFalse(got[0].IsTag);
      Aver.AreEqual("How is ", got[0].Content);
      Aver.AreEqual(0, got[0].IdxStart);
      Aver.AreEqual(6, got[0].IdxEnd);
      Aver.AreEqual(7, got[0].Length);

      Aver.IsTrue(got[1].IsTag);
      Aver.AreEqual("monster", got[1].Content);
      Aver.AreEqual(7, got[1].IdxStart);
      Aver.AreEqual(14, got[1].IdxEnd);
      Aver.AreEqual(8, got[1].Length);//with < >
    }

    [Run]
    public void ParseSegments_03()
    {
      var got = "<monster> How is".ParseSegments().ToArray();

      got.See();

      Aver.AreEqual(2, got.Length);

      Aver.IsTrue(got[0].IsTag);
      Aver.AreEqual("monster", got[0].Content);
      Aver.AreEqual(0, got[0].IdxStart);
      Aver.AreEqual(8, got[0].IdxEnd);
      Aver.AreEqual(9, got[0].Length);//with < >

      Aver.IsFalse(got[1].IsTag);
      Aver.AreEqual(" How is", got[1].Content);
      Aver.AreEqual(9, got[1].IdxStart);
      Aver.AreEqual(15, got[1].IdxEnd);
      Aver.AreEqual(7, got[1].Length);

    }

  }
}
