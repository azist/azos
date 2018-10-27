/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/
 
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Azos.IO.FileSystem.S3.V4;
using Azos.Scripting;

namespace Azos.Tests.Unit.IO.FileSystem.S3.V4
{
  [Runnable]
  public class SeparateLocalPathTest
  {
    [Run]
    public void TestEmpty()
    {
      string parent, name;

      S3V4FileSystem.S3V4FSH.SeparateLocalPath(null, out parent, out name);
      Aver.IsNull(parent);
      Aver.IsTrue(name == string.Empty);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("", out parent, out name);
      Aver.IsNull(parent);
      Aver.IsTrue(name == string.Empty);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/", out parent, out name);
      Aver.IsNull(parent);
      Aver.IsTrue(name == string.Empty);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath(" /  ", out parent, out name);
      Aver.IsNull(parent);
      Aver.IsTrue(name == string.Empty);
    }

    [Run]
    public void TestRootOnly()
    {
      string parent, name;

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/root", out parent, out name);
      Aver.IsTrue(parent == string.Empty);
      Aver.AreEqual("root", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("root/", out parent, out name);
      Aver.IsTrue(parent == string.Empty);
      Aver.AreEqual("root", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/root/", out parent, out name);
      Aver.IsTrue(parent == string.Empty);
      Aver.AreEqual("root", name);
    }

    [Run]
    public void TestRootNChild()
    {
      string parent, name;

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/root/child", out parent, out name);
      Aver.AreEqual("root", parent);
      Aver.AreEqual("child", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("root/child/", out parent, out name);
      Aver.AreEqual("root", parent);
      Aver.AreEqual("child", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/root/child/", out parent, out name);
      Aver.AreEqual("root", parent);
      Aver.AreEqual("child", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/ root//child/ ", out parent, out name);
      Aver.AreEqual("root", parent);
      Aver.AreEqual("child", name);
    }

    [Run]
    public void TestParentNChild()
    {
      string parent, name;

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/root/child/baby", out parent, out name);
      Aver.AreEqual("root/child", parent);
      Aver.AreEqual("baby", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("root/child/baby/", out parent, out name);
      Aver.AreEqual("root/child", parent);
      Aver.AreEqual("baby", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath("/root/child/baby/", out parent, out name);
      Aver.AreEqual("root/child", parent);
      Aver.AreEqual("baby", name);

      S3V4FileSystem.S3V4FSH.SeparateLocalPath(" /root/child//baby/", out parent, out name);
      Aver.AreEqual("root/child", parent);
      Aver.AreEqual("baby", name);
    }
  }
}
