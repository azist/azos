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

    [Run]
    public void ToLinuxLines()
    {
      Aver.AreEqual( "I walk\n lines\n\n", "I walk\r\n lines\r\n\r\n".ToLinuxLines() );
    }

    [Run]
    public void ToWindowsLines_FromLinux()
    {
      Aver.AreEqual("I walk\r\n lines\r\n\r\n", "I walk\n lines\n\n".ToWindowsLines());
    }

    [Run]
    public void ToWindowsLines_FromMixed()
    {
      Aver.AreEqual("I walk\r\n lines\r\n\r\n", "I walk\n lines\r\n\n".ToWindowsLines());
    }

    [Run]
    public void ToDumpString_0()
    {
      Aver.AreEqual(null, ((string)null).ToDumpString());
      Aver.AreEqual("", "".ToDumpString());
    }


    [Run]
    public void ToDumpString_1()
    {
      Aver.AreEqual(
@"0000: 49 20 77 61 6C 6B 0A 20 | 6C 69 6E 65 73 0D 0A 0A    I·walk··lines···
0010: 20 62 75 74 20 6E 6F 74 | 20 74 68 69 73 20 74 68    ·but·not·this·th
0020: 65 6E 20 68 61 73 20 77 | 68 61 74 20 6D 6F 72 65    en·has·what·more
0030: 20 63 68 61 72 61 63 74 | 65 72 73 20 6C 6F 6E 67    ·characters·long
0040: 65 72 20 6C 69 6E 65 73 | 20 67 6F 65 73 20 68 61    er·lines·goes·ha
0050: 73 20 64 69 64 20 68 61 | 76 65 20 6E 6F 74 20 67    s·did·have·not·g
0060: 65 74 20 69 73 20 6D 61 | 79 20 62 65 20 74 68 65    et·is·may·be·the
0070: 6E 20 64 6F 65 73                                    n·does",
      "I walk\n lines\r\n\n but not this then has what more characters longer lines goes has did have not get is may be then does".ToDumpString());
    }

    [Run]
    public void ToDumpString_2()
    {
      "\n{0}\n".SeeArgs("I walk\n lines\r\n\n but not this then has what more characters longer lines goes has did have not get is may be then does".ToDumpString());
    }

    [Run]
    public void ToDumpString_3()
    {
      Aver.AreEqual("0000: 53 68 6F 72 74 20 6C 69 | 6E 65                      Short·line", "Short line".ToDumpString());
    }

    [Run]
    public void ToDumpString_4()
    {
      Aver.AreEqual(
@"0000: 042F 0020 043C 043E 0433 0443 0020 0435 | 0441 0442 044C 0020 0441 0442 0435 043A    Я·могу·есть·стек
0010: 043B 043E 002C 0020 043E 043D 043E 0020 | 043C 043D 0435 0020 043D 0435 0020 0432    ло,·оно·мне·не·в
0020: 0440 0435 0434 0438 0442 002E                                                        редит.",
"Я могу есть стекло, оно мне не вредит.".ToDumpString());
    }


    [Run]
    public void DiffStrings_1()
    {
      var diff = "abcd".DiffStrings("abcd");
      Console.WriteLine( diff );
      Aver.IsTrue( diff.Contains("Identical"));
    }

    [Run]
    public void DiffStrings_2()
    {
      var diff = "abcde".DiffStrings("abcd");
      Console.WriteLine(diff);
      Aver.IsTrue(diff.Contains("A is longer than B by 1 chars"));
    }

    [Run]
    public void DiffStrings_3()
    {
      var diff = "abcd".DiffStrings("abcde");
      Console.WriteLine(diff);
      Aver.IsTrue(diff.Contains("B is longer than A by 1 chars"));
    }

    [Run]
    public void DiffStrings_4()
    {
      Console.WriteLine("abcd\nef".DiffStrings("ab\n\rcdef"));
    }

    [Run]
    public void DiffStrings_5()
    {
      Console.WriteLine("ab8-0er7t-98e7wr-9t87ew-98r7t-98e7wrtcd\nef".DiffStrings("ab\n\r3453485rcdef", 5));
    }

    [Run]
    public void DiffStrings_6()
    {
      Console.WriteLine("abc".DiffStrings(null));
    }

    [Run]
    public void DiffStrings_7()
    {
      Console.WriteLine(((string)null).DiffStrings("abc"));
    }

    [Run]
    public void DiffStrings_8()
    {
      Console.WriteLine(((string)null).DiffStrings((string)null));
    }

    [Run]
    public void TrimAll_1()
    {
      Aver.AreEqual("abcd", "\ra      b\n\n\n\nc d\r\r   ".TrimAll());
    }

    [Run]
    public void TrimAll_2()
    {
      Aver.AreEqual("bcd", "aa aba   aaa acaa aaa adaaaaa aaaaaa".TrimAll('a', ' '));
    }

    [Run]
    public void SplitKVP_1()
    {
      var got = "key=value".SplitKVP();
      Aver.AreEqual("key", got.Key);
      Aver.AreEqual("value", got.Value);
    }


    [Run]
    public void SplitKVP_2()
    {
      var got = "key=value".SplitKVP('-');
      Aver.AreEqual("key=value", got.Key);
      Aver.AreEqual("", got.Value);
    }

    [Run]
    public void SplitKVP_3()
    {
      var got = "key=value=9".SplitKVP('=');
      Aver.AreEqual("key", got.Key);
      Aver.AreEqual("value=9", got.Value);
    }

    [Run]
    public void SplitKVP_4()
    {
      var got = "key=value".SplitKVP(':','=');
      Aver.AreEqual("key", got.Key);
      Aver.AreEqual("value", got.Value);

      got = "key:1=value:1".SplitKVP('=', ':');
      Aver.AreEqual("key:1", got.Key);
      Aver.AreEqual("value:1", got.Value);
    }

    [Run]
    public void SplitKVP_5()
    {
      var got = "key=value".SplitKVP(':', '=', '-');
      Aver.AreEqual("key", got.Key);
      Aver.AreEqual("value", got.Value);

      got = "key:value".SplitKVP(':', '=', '-');
      Aver.AreEqual("key", got.Key);
      Aver.AreEqual("value", got.Value);

      got = "key-value".SplitKVP(':', '=', '-');
      Aver.AreEqual("key", got.Key);
      Aver.AreEqual("value", got.Value);
    }

    [Run]
    public void SplitKVP_6()
    {
      var got = "".SplitKVP('=');
      Aver.AreEqual("", got.Key);
      Aver.AreEqual("", got.Value);
    }

    [Run]
    public void SplitKVP_7()
    {
      var got = "key=".SplitKVP('=');
      Aver.AreEqual("key", got.Key);
      Aver.AreEqual("", got.Value);
    }

    [Run]
    public void SplitKVP_8()
    {
      var got = "=value".SplitKVP('=');
      Aver.AreEqual("", got.Key);
      Aver.AreEqual("value", got.Value);
    }

    [Run] public void TakeFirstChars_1() => Aver.AreEqual(null, ((string)null).TakeFirstChars(100));
    [Run] public void TakeFirstChars_2() => Aver.AreEqual("",  "".TakeFirstChars(100));
    [Run] public void TakeFirstChars_3() => Aver.AreEqual("a2", "a2".TakeFirstChars(100));
    [Run] public void TakeFirstChars_4() => Aver.AreEqual("a23", "a2345".TakeFirstChars(3));
    [Run] public void TakeFirstChars_5() => Aver.AreEqual("a2345", "a23457890".TakeFirstChars(5));
    [Run] public void TakeFirstChars_6() => Aver.AreEqual("a2345", "a23457890".TakeFirstChars(5, ""));
    [Run] public void TakeFirstChars_7() => Aver.AreEqual("a23..", "a23457890".TakeFirstChars(5, ".."));
    [Run] public void TakeFirstChars_8() => Aver.AreEqual("", "a23457890".TakeFirstChars(-5));
    [Run] public void TakeFirstChars_9() => Aver.AreEqual(".....", "a23457890".TakeFirstChars(5, "..............."));
    [Run] public void TakeFirstChars_10() => Aver.AreEqual("1234", "1234".TakeFirstChars(4));
    [Run] public void TakeFirstChars_11() => Aver.AreEqual("12..", "12345".TakeFirstChars(4, ".."));

    [Run] public void TakeLastChars_1() => Aver.AreEqual(null, ((string)null).TakeLastChars(100));
    [Run] public void TakeLastChars_2() => Aver.AreEqual("", "".TakeLastChars(100));
    [Run] public void TakeLastChars_3() => Aver.AreEqual("a2", "a2".TakeLastChars(100));
    [Run] public void TakeLastChars_4() => Aver.AreEqual("345", "a2345".TakeLastChars(3));
    [Run] public void TakeLastChars_5() => Aver.AreEqual("57890", "a23457890".TakeLastChars(5));
    [Run] public void TakeLastChars_6() => Aver.AreEqual("57890", "a23457890".TakeLastChars(5, ""));
    [Run] public void TakeLastChars_7() => Aver.AreEqual("..890", "a23457890".TakeLastChars(5, ".."));
    [Run] public void TakeLastChars_8() => Aver.AreEqual("", "a23457890".TakeLastChars(-5));
    [Run] public void TakeLastChars_9() => Aver.AreEqual(".....", "a23457890".TakeLastChars(5, "..............."));
    [Run] public void TakeLastChars_10() => Aver.AreEqual("1234", "1234".TakeLastChars(4));
    [Run] public void TakeLastChars_11() => Aver.AreEqual("..45", "12345".TakeLastChars(4, ".."));


    [Run] public void TakeLastSegment_1() => Aver.AreEqual(null, ((string)null).TakeLastSegment('/'));
    [Run] public void TakeLastSegment_2() => Aver.AreEqual("", "".TakeLastSegment('/'));
    [Run] public void TakeLastSegment_3() => Aver.AreEqual("", "a/".TakeLastSegment('/'));
    [Run] public void TakeLastSegment_4() => Aver.AreEqual("here", "snake.zhaba/here".TakeLastSegment('/'));
    [Run] public void TakeLastSegment_5() => Aver.AreEqual("zhaba/here", "snake.zhaba/here".TakeLastSegment('.'));

  }
}