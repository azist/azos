/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


using System;
using System.Collections.Generic;

using Azos.Scripting;

namespace Azos.Tests.Nub
{
  [Runnable]
  public class StrUtilsTests
  {
    [Run]
    public void ArgsTest()
    {
      Aver.AreEqual("a=1 b=2", "a={0} b={1}".Args(1,2));
      Aver.AreEqual("a=789 b=", "a={0} b={1}".Args(789, null));
    }

    [Run]
    public void IsOneOf_array()
    {
      Aver.IsTrue( "abc".IsOneOf("abc") );
      Aver.IsTrue( "abc".IsOneOf("def", "abc"));
      Aver.IsTrue(((string)null).IsOneOf("abc", null));
      Aver.IsTrue("abC".IsOneOf("aBc"));
      Aver.IsTrue("zUZA".IsOneOf("aBc","def","weqrqwrqwrqwer","zuza"));

      Aver.IsFalse(((string)null).IsOneOf(null));
      Aver.IsFalse("aaa".IsOneOf(null));
      Aver.IsFalse(((string)null).IsOneOf("abc", "def"));
      Aver.IsFalse("def".IsOneOf("abc", "fed", "diff"));

      Aver.IsFalse("  def  ".IsOneOf("abc", "def", "diff"));
      Aver.IsFalse("def".IsOneOf("abc", "  def  ", "diff"));
      Aver.IsTrue("def".IsOneOf("abc", "def", "diff"));
    }

    [Run]
    public void IsOneOf_Enumerable()
    {
      Aver.IsTrue("abc".IsOneOf(new List<string>{"abc"}));
      Aver.IsTrue("abc".IsOneOf(new List<string> { "def", "abc"}));
      Aver.IsTrue(((string)null).IsOneOf(new List<string> { "abc", null}));
      Aver.IsTrue("abC".IsOneOf(new List<string> { "aBc"}));
      Aver.IsTrue("zUZA".IsOneOf(new List<string> { "aBc", "def", "weqrqwrqwrqwer", "zuza"}));

      Aver.IsFalse( ((string)null).IsOneOf((List<string>)null));
      Aver.IsFalse("aaa".IsOneOf((List<string>)  null));
      Aver.IsFalse(((string)null).IsOneOf(new List<string> { "abc", "def"}));
      Aver.IsFalse("def".IsOneOf(new List<string> { "abc", "fed", "diff"}));

      Aver.IsFalse("  def  ".IsOneOf(new List<string> { "abc", "def", "diff"}));
      Aver.IsFalse("def".IsOneOf(new List<string> { "abc", "  def  ", "diff"}));
      Aver.IsTrue("def".IsOneOf(new List<string> { "abc", "def", "diff"}));
    }
  }
}