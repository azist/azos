/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Linq;

using Azos.Scripting;
using Azos.Serialization.JSON;
using Azos.CodeAnalysis.Source;
using Azos.Serialization.JSON.Backends;
using Azos.CodeAnalysis.JSON;

namespace Azos.Tests.Nub.Serialization
{
  [Runnable]
  public class JazonLexerTests
  {
    [Run]
    public void LexerTest()
    {
      var json = @"{ a:       1, b: ""something"", c: null, d: {}, e: 23.7}";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();

      got.Select(t => "{0} `{1}`".Args(t.Type, t.Text) ).See();

      Aver.AreEqual(24, got.Length);

      Aver.IsTrue(JsonTokenType.tBOF == got[0].Type );

      Aver.IsTrue(JsonTokenType.tBraceOpen == got[1].Type);
      Aver.AreEqual("{", got[1].Text);

      Aver.IsTrue(JsonTokenType.tIdentifier == got[2].Type);
      Aver.AreEqual("a", got[2].Text);

      Aver.IsTrue(JsonTokenType.tColon == got[3].Type);
      Aver.AreEqual(":", got[3].Text);

      Aver.IsTrue(JsonTokenType.tIntLiteral == got[4].Type);
      Aver.AreEqual("1", got[4].Text);
      Aver.AreEqual(1ul, got[4].ULValue);

      Aver.IsTrue(JsonTokenType.tComma == got[5].Type);
      Aver.AreEqual(",", got[5].Text);

      Aver.IsTrue(JsonTokenType.tIdentifier == got[6].Type);
      Aver.AreEqual("b", got[6].Text);

      Aver.IsTrue(JsonTokenType.tColon == got[7].Type);
      Aver.AreEqual(":", got[7].Text);

      Aver.IsTrue(JsonTokenType.tStringLiteral == got[8].Type);
      Aver.AreEqual("something", got[8].Text);

      Aver.IsTrue(JsonTokenType.tComma == got[9].Type);
      Aver.AreEqual(",", got[9].Text);

      Aver.IsTrue(JsonTokenType.tIdentifier == got[10].Type);
      Aver.AreEqual("c", got[10].Text);

      Aver.IsTrue(JsonTokenType.tColon == got[11].Type);
      Aver.AreEqual(":", got[11].Text);

      Aver.IsTrue(JsonTokenType.tNull == got[12].Type);
      Aver.AreEqual("null", got[12].Text);

      Aver.IsTrue(JsonTokenType.tComma == got[13].Type);
      Aver.AreEqual(",", got[13].Text);

      Aver.IsTrue(JsonTokenType.tIdentifier == got[14].Type);
      Aver.AreEqual("d", got[14].Text);

      Aver.IsTrue(JsonTokenType.tColon == got[15].Type);
      Aver.AreEqual(":", got[15].Text);

      Aver.IsTrue(JsonTokenType.tBraceOpen == got[16].Type);
      Aver.AreEqual("{", got[16].Text);

      Aver.IsTrue(JsonTokenType.tBraceClose == got[17].Type);
      Aver.AreEqual("}", got[17].Text);

      Aver.IsTrue(JsonTokenType.tComma == got[18].Type);
      Aver.AreEqual(",", got[18].Text);

      Aver.IsTrue(JsonTokenType.tIdentifier == got[19].Type);
      Aver.AreEqual("e", got[19].Text);

      Aver.IsTrue(JsonTokenType.tColon == got[20].Type);
      Aver.AreEqual(":", got[20].Text);

      Aver.IsTrue(JsonTokenType.tDoubleLiteral == got[21].Type);
      Aver.AreEqual("23.7", got[21].Text);
      Aver.AreEqual(23.7d, got[21].DValue);

      Aver.IsTrue(JsonTokenType.tBraceClose == got[22].Type);
      Aver.AreEqual("}", got[22].Text);

      Aver.IsTrue(JsonTokenType.tEOF == got[23].Type);
    }


    [Run]
    public void Int_1()
    {
      var json = "1234";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.Skip(1).First();
      Aver.IsTrue(JsonTokenType.tIntLiteral == got.Type);
      Aver.AreEqual(1234ul, got.ULValue);
    }

    [Run]
    public void Int_2()
    {
      var json = "2000000000";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.Skip(1).First();
      Aver.IsTrue(JsonTokenType.tIntLiteral == got.Type);
      Aver.AreEqual(2_000_000_000ul, got.ULValue);
    }

    [Run]
    public void Int_3()
    {
      var json = "3000000000";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.Skip(1).First();
      Aver.IsTrue(JsonTokenType.tLongIntLiteral == got.Type);
      Aver.AreEqual(3_000_000_000ul, got.ULValue);
    }

    [Run]
    public void Int_4()
    {
      var json = "19446744073709551616‬";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.Skip(1).First();//throws
      got.See();
      Aver.IsTrue(-1 == (int)got.Type);
      Aver.IsTrue(JsonMsgCode.eValueTooBig == (JsonMsgCode)got.ULValue);
    }

    [Run]
    public void Int_5()
    {
      var json = "18446744073709551615";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.Skip(1).First();
      Aver.IsTrue(JsonTokenType.tLongIntLiteral == got.Type);
      Aver.AreEqual(ulong.MaxValue, got.ULValue);
    }


    [Run]
    public void Doubles_1()
    {
      var json = "3.1415987";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.Skip(1).First();
      Aver.IsTrue(JsonTokenType.tDoubleLiteral == got.Type);
      Aver.AreEqual(3.1415987d, got.DValue);
    }

    [Run]
    public void Doubles_2()
    {
      var json = "3e2";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.Skip(1).First();
      Aver.IsTrue(JsonTokenType.tDoubleLiteral == got.Type);
      Aver.AreEqual(3e2d, got.DValue);
    }

    [Run]
    public void Doubles_3()
    {
      var json = "3e-2";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.Skip(1).First();
      Aver.IsTrue(JsonTokenType.tDoubleLiteral == got.Type);
      Aver.AreEqual(3e-2d, got.DValue);
    }

    [Run]
    public void Doubles_4()
    {
      var json = "3e+2";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.Skip(1).First();
      Aver.IsTrue(JsonTokenType.tDoubleLiteral == got.Type);
      Aver.AreEqual(3e+2d, got.DValue);
    }


    [Run]
    public void Comments_1()
    {
      var json = "// comment line";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tComment == got[1].Type);
      Aver.AreEqual(" comment line", got[1].Text);
    }

    [Run]
    public void Comments_2()
    {
      var json = "/* comment block line 2 */";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tComment == got[1].Type);
      Aver.AreEqual(" comment block line 2 ", got[1].Text);
    }

    [Run]
    public void Comments_3()
    {
      var json = "                                          /* comment block line 3 */  ";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tComment == got[1].Type);
      Aver.AreEqual(" comment block line 3 ", got[1].Text);
    }

    [Run]
    public void Comments_4()
    {
      var json = @"  /* comment 
      block 
      span */  ";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tComment == got[1].Type);
      Aver.AreEqual("commentblockspan", got[1].Text.TrimAll(' ','\n','\r'));
    }

    [Run]
    public void Comments_5()
    {
      var json = @"  /* comment 
      block ""not a string""
      span */  ";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tComment == got[1].Type);
      Aver.AreEqual("commentblock\"notastring\"span", got[1].Text.TrimAll(' ', '\n', '\r'));
    }

    [Run]
    public void Comments_6()
    {
      var json = @"  /* comment 
      block 'not a string'
      span */  ";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tComment == got[1].Type);
      Aver.AreEqual("commentblock'notastring'span", got[1].Text.TrimAll(' ', '\n', '\r'));
    }

    [Run]
    public void Comments_7()
    {
      var json = @"  /* comment 
      block 'not a string' // this is not a comment
      span */  ";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tComment == got[1].Type);
      Aver.AreEqual("commentblock'notastring'//thisisnotacommentspan", got[1].Text.TrimAll(' ', '\n', '\r'));
    }

    [Run]
    public void Comments_8()
    {
      var json = @"  /* comment 
      block 'not a\u223322 string' // this is not a comment
      span */  ";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tComment == got[1].Type);
      Aver.AreEqual(@"commentblock'nota\u223322string'//thisisnotacommentspan", got[1].Text.TrimAll(' ', '\n', '\r'));
    }

    [Run]
    public void Strings_1()
    {
      var json = "\"abc\"";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tStringLiteral == got[1].Type);
      Aver.AreEqual(@"abc", got[1].Text);
    }

    [Run]
    public void Strings_2()
    {
      var json = "'abc'";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tStringLiteral == got[1].Type);
      Aver.AreEqual(@"abc", got[1].Text);
    }

    [Run]
    public void Strings_3()
    {
      var json = "'ab\"c'";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tStringLiteral == got[1].Type);
      Aver.AreEqual(@"ab""c", got[1].Text);
    }

    [Run]
    public void Strings_4()
    {
      var json = "\"ab'c\"";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tStringLiteral == got[1].Type);
      Aver.AreEqual(@"ab'c", got[1].Text);
    }

    [Run]
    public void Strings_5()
    {
      var json = "\"new\\nline\"";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tStringLiteral == got[1].Type);
      Aver.AreEqual("new\nline", got[1].Text);
    }

    [Run]
    public void Strings_6()
    {
      var json = "\"new\\rline\"";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tStringLiteral == got[1].Type);
      Aver.AreEqual("new\rline", got[1].Text);
    }

    [Run]
    public void Strings_7()
    {
      var json = "\"this is not // a comment\"";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tStringLiteral == got[1].Type);
      Aver.AreEqual("this is not // a comment", got[1].Text);
    }

    [Run]
    public void Strings_8()
    {
      var json = "\"this is not /* a comment */ either\"";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tStringLiteral == got[1].Type);
      Aver.AreEqual("this is not /* a comment */ either", got[1].Text);
    }

    [Run]
    public void Strings_9()
    {
      var json = @"$""this is not /* a
      comment */
      either""";
      var src = new StringSource(json);
      var lxr = new JazonLexer(src);

      var got = lxr.ToArray();
      got.See();
      Aver.IsTrue(JsonTokenType.tStringLiteral == got[1].Type);
      Aver.AreEqual("thisisnot/*acomment*/either", got[1].Text.TrimAll(' ', '\n', '\r'));
    }

  }
}

