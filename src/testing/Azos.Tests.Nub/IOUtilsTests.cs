/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class IOUtilsTests
  {
    [Run("a='0'")]
    [Run("a='0,0,0'")]
    [Run("a='255,255,255'")]
    [Run("a='255,255,255,255'")]
    [Run("a='255,255,255,0'")]
    [Run("a='1'")]
    [Run("a='255'")]
    [Run("a='127'")]
    [Run("a='0,255'")]
    [Run("a='1,2,3,4,5,6,7,8,9,0'")]
    [Run("a='1,2,3,4,5,6,7,8,9,254,255'")]
    [Run("a='1,2,3,4,5,6,7,8,9,254,255'")]
    public void Base64FullCycle(byte[] a)
    {
      var encoded = a.ToWebSafeBase64();
      var b = encoded.FromWebSafeBase64();

      //Console.WriteLine( a.ToDumpString(DumpFormat.Hex ));
      //Console.WriteLine(b.ToDumpString(DumpFormat.Hex));
      //Console.WriteLine(encoded);

      Aver.AreArraysEquivalent(a, b);
    }

    [Run]
    public void Base64FullCycle_Loop()
    {
      for(var cnt=1; cnt<1024; cnt++)
      {
        var a = Platform.RandomGenerator.Instance.NextRandomBytes(cnt);
        Base64FullCycle(a);
      }
    }
  }
}