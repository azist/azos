/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Collections;
using Azos.Scripting;

namespace Azos.Tests.Nub.BaseCollections
{
  [Runnable]
  public class RefEquTests
  {
    [Run]
    public void Test1()
    {
      object a = 1;
      object b = 1;
      object c = a;
      Aver.IsFalse(ReferenceEqualityComparer<object>.Instance.Equals(a, b));
      Aver.IsFalse(ReferenceEqualityComparer<object>.Instance.Equals(b, a));
      Aver.IsTrue(ReferenceEqualityComparer<object>.Instance.Equals(a, c));
      Aver.IsTrue(ReferenceEqualityComparer<object>.Instance.Equals(c, a));

      Aver.AreNotEqual(ReferenceEqualityComparer<object>.Instance.GetHashCode(a), ReferenceEqualityComparer<object>.Instance.GetHashCode(b));
      Aver.AreEqual(ReferenceEqualityComparer<object>.Instance.GetHashCode(a), ReferenceEqualityComparer<object>.Instance.GetHashCode(c));
    }

    [Run]
    public void Test2()
    {
      string a = "a";
      string b = "b";
      string c = a;
      Aver.IsFalse(ReferenceEqualityComparer<string>.Instance.Equals(a, b));
      Aver.IsFalse(ReferenceEqualityComparer<string>.Instance.Equals(b, a));
      Aver.IsTrue(ReferenceEqualityComparer<string>.Instance.Equals(a, c));
      Aver.IsTrue(ReferenceEqualityComparer<string>.Instance.Equals(c, a));

      Aver.AreNotEqual(ReferenceEqualityComparer<string>.Instance.GetHashCode(a), ReferenceEqualityComparer<string>.Instance.GetHashCode(b));
      Aver.AreEqual(ReferenceEqualityComparer<string>.Instance.GetHashCode(a), ReferenceEqualityComparer<string>.Instance.GetHashCode(c));
    }

  }
}
