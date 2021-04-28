/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

using Azos.Scripting;
using Azos.Security;

namespace Azos.Tests.Nub.Security
{
  [Runnable]
  public class AreStringsEqualInLengthConstantTimeTests
  {
    [Run("a='' b='' eq=true")]
    [Run("a='a' b='a' eq=true")]
    [Run("a='abc' b='abc' eq=true")]
    [Run("a='abcd' b='abc' eq=false")]
    [Run("a='abc' b='abcd' eq=false")]
    [Run("a='aBc' b='abc' eq=false")]
    [Run("a='abcdef ABCDEF' b='abcdef ABCDEF' eq=true")]
    public void Strings(string a, string b, bool eq)
    => Aver.AreEqual(eq, HashedPassword.AreStringsEqualInLengthConstantTime(a, b));

    [Run("cnt=10")]
    [Run("cnt=20")]
    [Run("cnt=48")]
    [Run("cnt=49")]
    [Run("cnt=59")]
    [Run("cnt=159")]
    [Run("cnt=5000")]
    public void StringLong(int cnt)
    {
      var a = "";
      var b = "";
      for(var i=0; i<cnt; i++)
      {
        a += i.ToString();
        b += i.ToString();
      }

      Aver.IsTrue(HashedPassword.AreStringsEqualInLengthConstantTime(a, b));

      a = "1" + Ambient.Random.NextRandomWebSafeString(cnt-1);
      b = "2" + Ambient.Random.NextRandomWebSafeString(cnt-1);
      Aver.IsFalse(HashedPassword.AreStringsEqualInLengthConstantTime(a, b));
    }


    [Run]
    public void Strings_Null()
    {
      Aver.IsTrue(HashedPassword.AreStringsEqualInLengthConstantTime((string)null, null));
      Aver.IsFalse(HashedPassword.AreStringsEqualInLengthConstantTime("", null));
      Aver.IsFalse(HashedPassword.AreStringsEqualInLengthConstantTime(null,""));
    }

    [Run("a='' b='' eq=true")]
    [Run("a='1' b='1' eq=true")]
    [Run("a='1,2,3' b='1,2,3' eq=true")]
    [Run("a='1,2,3,4' b='1,2,3' eq=false")]
    [Run("a='1,2,3' b='1,2,3,4' eq=false")]
    [Run("a='1,4,5' b='1,5,4' eq=false")]
    [Run("a='1,2,3,0,0,100' b='1,2,3,0,0,100' eq=true")]
    public void Bytes(byte[] a, byte[] b, bool eq)
    => Aver.AreEqual(eq, HashedPassword.AreStringsEqualInLengthConstantTime(a, b));

    [Run("cnt=10")]
    [Run("cnt=20")]
    [Run("cnt=48")]
    [Run("cnt=49")]
    [Run("cnt=59")]
    [Run("cnt=159")]
    [Run("cnt=5000")]
    public void BytesLong(int cnt)
    {
      var a = new byte[cnt];
      var b = new byte[cnt];
      for (var i = 0; i < cnt; i++)
      {
        a[i] = (byte)i;
        b[i] = (byte)i;
      }

      Aver.IsTrue(HashedPassword.AreStringsEqualInLengthConstantTime(a, b));

      a = ((byte)1).ConcatArray(Ambient.Random.NextRandomBytes(cnt - 1));
      b = ((byte)2).ConcatArray(Ambient.Random.NextRandomBytes(cnt - 1));
      Aver.IsFalse(HashedPassword.AreStringsEqualInLengthConstantTime(a, b));
    }

    [Run]
    public void Bytes_Null()
    {
      Aver.IsTrue(HashedPassword.AreStringsEqualInLengthConstantTime((byte[])null, null));
      Aver.IsFalse(HashedPassword.AreStringsEqualInLengthConstantTime(new byte[0], null));
      Aver.IsFalse(HashedPassword.AreStringsEqualInLengthConstantTime(null, new byte[0]));
    }

    [Run]
    [MethodImpl(MethodImplOptions.NoOptimization)]
    public void ContrastStrings()
    {
      const int cnt = 250_000;

      var a = "this is an example of a string called A";
      var b = "B is different in first char already   ";

      var r = 0;
      var swEquals = Stopwatch.StartNew();
      for (var i=0; i<cnt; i++)
      {
        if (!string.Equals(a, b)) r++;
      }
      swEquals.Stop();

      var swSlow = Stopwatch.StartNew();
      for (var i = 0; i < cnt; i++)
      {
        if (!HashedPassword.AreStringsEqualInLengthConstantTime(a, b)) r++;
      }
      swSlow.Stop();

      Aver.IsTrue(swEquals.ElapsedMilliseconds < swSlow.ElapsedMilliseconds);
      "string.Equals: {0}  Slow: {1}".SeeArgs(swEquals.ElapsedMilliseconds, swSlow.ElapsedMilliseconds);
    }

  }
}


