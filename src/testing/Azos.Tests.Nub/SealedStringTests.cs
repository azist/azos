/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Threading.Tasks;

using Azos.Text;
using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class SealedStringTests
  {
    [Run]
    public void Unassigned()
    {
      var empty = new SealedString();

      Aver.IsFalse(empty.IsAssigned);

      empty = SealedString.Unassigned;

      Aver.IsFalse(empty.IsAssigned);

      Aver.AreEqual(SealedString.Unassigned, new SealedString());
      Aver.AreEqual(0, SealedString.Unassigned.GetHashCode());

      Aver.IsTrue(null == empty.Value);
    }

    [Run]
    public void Create()
    {
      var s1 = new SealedString("Lenin");

      var original = s1.Value;

      Aver.IsTrue(s1.IsAssigned);
      Aver.AreEqual("Lenin", original);
      s1.ToString().See();
    }

    [Run("cnt=10")]
    [Run("cnt=100")]
    [Run("cnt=250")]
    [Run("cnt=1250")]
    [Run("cnt=32000")]
    [Run("cnt=132000")]
    public void VariousSizesOfWideChars(int cnt)
    {
      var original = new string('久', cnt);

      var sld = new SealedString(original);

      var got = sld.Value;

      Aver.IsTrue(sld.IsAssigned);
      Aver.AreEqual(original, got);
    }

    [Run]
    public void LongMulticulturalString()
    {
      var original = "就是巴尼宝贝儿吧，俺说。有什么怪事儿或是好事儿吗？ когда американские авианосцы 'Уинсон' и 'Мидуэй' приблизились 지구상의　３대 we have solved the problem";

      var sld = new SealedString(original);

      var got = sld.Value;

      Aver.IsTrue(sld.IsAssigned);
      Aver.AreEqual(original, got);
    }

    [Run]
    public void Equals()
    {
      var s1 = new SealedString("Bird");
      var s2 = new SealedString("Cat");

      Aver.IsTrue(s1.IsAssigned);
      Aver.IsTrue(s2.IsAssigned);
      Aver.AreEqual("Bird", s1.Value);
      Aver.AreEqual("Cat", s2.Value);

      Aver.AreNotEqual(s1, s2);
      Aver.IsFalse(s1 == s2);
      Aver.IsTrue(s1 != s2);
      Aver.AreNotEqual(s1.GetHashCode(), s2.GetHashCode());

      var s3 = SealedString.Unassigned;
      Aver.AreNotEqual(s1, s3);
      Aver.IsFalse(s1 == s3);
      Aver.IsTrue(s1 != s3);
      Aver.AreNotEqual(s1.GetHashCode(), s3.GetHashCode());

      s3 = s1;
      Aver.AreEqual(s1, s3);
      Aver.IsTrue(s1 == s3);
      Aver.IsFalse(s1 != s3);
      Aver.AreEqual(s1.GetHashCode(), s3.GetHashCode());
    }

    [Run]
    public void Counts()
    {
      const int cnt = 512000;

      var startCount = SealedString.TotalCount;
      var startUseCount = SealedString.TotalBytesUsed;
      var startAllocCount = SealedString.TotalBytesAllocated;

      "Total: {0:n0} / used bytes: {1:n0} / allocated: {2:n0}".SeeArgs(startCount, startUseCount, startAllocCount);

      var sw = System.Diagnostics.Stopwatch.StartNew();
      for (var i = 0; i < cnt; i++) new SealedString("String content that is not very short but not very long either");
      sw.Stop();

      var endCount = SealedString.TotalCount;
      var endUseCount = SealedString.TotalBytesUsed;
      var endAllocCount = SealedString.TotalBytesAllocated;

      Aver.AreEqual(startCount + cnt, endCount);
      Aver.IsTrue(endUseCount > startUseCount);
      Aver.IsTrue(endAllocCount >= startAllocCount);

      "Total: {0:n0} / used bytes: {1:n0} / allocated: {2:n0}".SeeArgs(endCount, endUseCount, endAllocCount);

      "Did {0:n0} in {1:n0} ms at {2:n0} ops/sec".SeeArgs(cnt, sw.ElapsedMilliseconds, cnt / (sw.ElapsedMilliseconds / 1000d));

      "Total segments: {0}".SeeArgs(SealedString.TotalSegmentCount);
    }

    [Run("cnt=712345  from=3     to=10")]
    [Run("cnt=798456  from=7     to=37")]
    [Run("cnt=12500  from=512  to=15000")]
    public void Multithreaded(int cnt, int from, int to)
    {
      var startCount = SealedString.TotalCount;
      var startUseCount = SealedString.TotalBytesUsed;
      var startAllocCount = SealedString.TotalBytesAllocated;

      var data = new string[1024];
      for (var i = 0; i < data.Length; i++)
        data[i] = "A".PadRight(from + Ambient.Random.NextScaledRandomInteger(0, to));

      "Total: {0:n0} / used bytes: {1:n0} / allocated: {2:n0}".SeeArgs(startCount, startUseCount, startAllocCount);

      var sw = System.Diagnostics.Stopwatch.StartNew();
      Parallel.For(0, cnt, (_) =>
      {
        var content = data[Ambient.Random.NextScaledRandomInteger(0, data.Length)];
        var s = new SealedString(content);
        var restored = s.Value;
        Aver.AreEqual(content, restored);
      });
      sw.Stop();

      var endCount = SealedString.TotalCount;
      var endUseCount = SealedString.TotalBytesUsed;
      var endAllocCount = SealedString.TotalBytesAllocated;

      Aver.AreEqual(startCount + cnt, endCount);
      Aver.IsTrue(endUseCount > startUseCount);
      Aver.IsTrue(endAllocCount >= startAllocCount);

      "Total: {0:n0} / used bytes: {1:n0} / allocated: {2:n0}".SeeArgs(endCount, endUseCount, endAllocCount);

      "Did {0:n0} in {1:n0} ms at {2:n0} ops/sec".SeeArgs(cnt, sw.ElapsedMilliseconds, cnt / (sw.ElapsedMilliseconds / 1000d));

      "Total segments: {0}".SeeArgs(SealedString.TotalSegmentCount);
    }

    [Run]
    public void Scope_Insensitive()
    {
      var scope = new SealedString.Scope();

      var s1 = scope.Seal("Lenin");
      var s2 = scope.Seal("has");
      var s3 = scope.Seal("LeNIN");
      var s4 = scope.Seal("LeNeN");

      Aver.AreEqual(s1, s3);
      Aver.AreNotEqual(s1, s2);
      Aver.AreNotEqual(s1, s4);

      Aver.AreEqual(3, scope.Count);
      Aver.AreEqual(s1, scope["LENIN"]);
      Aver.AreEqual(s2, scope["HAS"]);
      Aver.AreEqual(s4, scope["LENEN"]);
    }

    [Run]
    public void Scope_Sensitive()
    {
      var scope = new SealedString.Scope(StringComparer.InvariantCulture);

      var s1 = scope.Seal("Lenin");
      var s2 = scope.Seal("has");
      var s3 = scope.Seal("LeNIN");
      var s4 = scope.Seal("LeNeN");

      Aver.AreNotEqual(s1, s3);
      Aver.AreNotEqual(s1, s2);
      Aver.AreNotEqual(s1, s4);

      Aver.AreEqual(4, scope.Count);
      Aver.AreEqual(s1, scope["Lenin"]);
      Aver.AreEqual(s2, scope["has"]);
      Aver.AreEqual(s3, scope["LeNIN"]);
      Aver.AreEqual(s4, scope["LeNeN"]);

      Aver.AreEqual(SealedString.Unassigned, scope["LENIN"]);
    }

  }
}
