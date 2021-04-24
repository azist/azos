/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System.Text;

using Azos.Scripting;
using Azos.CodeAnalysis;
using Azos.CodeAnalysis.Source;
using Azos.CodeAnalysis.Laconfig;

namespace Azos.Tests.Nub.CodeAnalysis
{
  [Runnable]
  public class LJSParserTests
  {
    [Run]
    public void Case_1()
    {
      var src = "div=divRoot{ }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual("divRoot", root.TranspilerPragma);
      Aver.AreEqual(0, root.Children.Length);
    }

    [Run]
    public void Case_2()
    {
      var src = "div=divRoot{ span1=s1{} span2{} }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual("divRoot", root.TranspilerPragma);
      Aver.AreEqual(2, root.Children.Length);

      Aver.AreEqual("span1", root.Children[0].Name);
      Aver.IsTrue(root.Children[0] is LJSSectionNode);
      Aver.AreEqual("s1", ((LJSSectionNode)root.Children[0]).TranspilerPragma);
      Aver.AreEqual(0, ((LJSSectionNode)root.Children[0]).Children.Length);

      Aver.AreEqual("span2", root.Children[1].Name);
      Aver.IsTrue(root.Children[1] is LJSSectionNode);
      Aver.IsNull(((LJSSectionNode)root.Children[1]).TranspilerPragma);
      Aver.AreEqual(0, ((LJSSectionNode)root.Children[1]).Children.Length);
    }

    [Run]
    public void Case_3()
    {
      var src = "div=divRoot{ span1=s1{} span2{ a{} b{} c{}} }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual("divRoot", root.TranspilerPragma);
      Aver.AreEqual(2, root.Children.Length);

      Aver.AreEqual("span1", root.Children[0].Name);
      Aver.IsTrue(root.Children[0] is LJSSectionNode);
      Aver.AreEqual("s1", ((LJSSectionNode)root.Children[0]).TranspilerPragma);
      Aver.AreEqual(0, ((LJSSectionNode)root.Children[0]).Children.Length);

      Aver.AreEqual("span2", root.Children[1].Name);
      Aver.IsTrue(root.Children[1] is LJSSectionNode);
      Aver.IsNull(((LJSSectionNode)root.Children[1]).TranspilerPragma);
      Aver.AreEqual(3, ((LJSSectionNode)root.Children[1]).Children.Length);

      Aver.AreEqual("a", ((LJSSectionNode)root.Children[1]).Children[0].Name);
      Aver.AreEqual("b", ((LJSSectionNode)root.Children[1]).Children[1].Name);
      Aver.AreEqual("c", ((LJSSectionNode)root.Children[1]).Children[2].Name);
    }

    [Run]
    public void Case_4()
    {
      var src = "div{  }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(0, root.Children.Length);
    }

    [Run]
    public void Case_5()
    {
      var src = "'div'{  }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(0, root.Children.Length);
    }

    [Run]
    public void Case_6()
    {
      var src = "div{ atr=val  }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(1, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSAttributeNode);
      Aver.AreEqual("atr", ((LJSAttributeNode)root.Children[0]).Name);
      Aver.AreEqual("val", ((LJSAttributeNode)root.Children[0]).Value);
    }

    [Run]
    public void Case_7()
    {
      var src = "div{ atr=val sub{ }  }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(2, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSAttributeNode);
      Aver.AreEqual("atr", ((LJSAttributeNode)root.Children[0]).Name);
      Aver.AreEqual("val", ((LJSAttributeNode)root.Children[0]).Value);

      Aver.IsTrue(root.Children[1] is LJSSectionNode);
      Aver.AreEqual("sub", ((LJSSectionNode)root.Children[1]).Name);
      Aver.AreEqual(0, ((LJSSectionNode)root.Children[1]).Children.Length);
    }

    [Run]
    public void Case_8()
    {
      var src = "div{ \"string content\" }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(1, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("string content", ((LJSContentNode)root.Children[0]).Content);
    }

    [Run]
    public void Case_9()
    {
      var src = "div{ string content }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(1, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("string content", ((LJSContentNode)root.Children[0]).Content);
    }

    [Run]
    public void Case_10()
    {
      var src = "div{ string \"content\" }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(1, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("string content", ((LJSContentNode)root.Children[0]).Content);
    }

    [Run]
    public void Case_11()
    {
      var src = "div{ string \"content\" subsection{} another text}";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(3, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("string content", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSSectionNode);
      Aver.AreEqual("subsection", ((LJSSectionNode)root.Children[1]).Name);

      Aver.IsTrue(root.Children[2] is LJSContentNode);
      Aver.AreEqual("another text", ((LJSContentNode)root.Children[2]).Content);
    }

    [Run]
    public void Case_11_1()
    {
      var src = @"div{ 
           string ""content""
           subsection{}
           another text}";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(3, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("string content", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSSectionNode);
      Aver.AreEqual("subsection", ((LJSSectionNode)root.Children[1]).Name);

      Aver.IsTrue(root.Children[2] is LJSContentNode);
      Aver.AreEqual("another text", ((LJSContentNode)root.Children[2]).Content);
    }

    [Run]
    public void Case_12()
    {
      var src =
@"
  div{
   string ""con = 2 - 3 tent""
   subsection{}
   here is another text about the writer of
  }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(3, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("string con = 2 - 3 tent", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSSectionNode);
      Aver.AreEqual("subsection", ((LJSSectionNode)root.Children[1]).Name);

      Aver.IsTrue(root.Children[2] is LJSContentNode);
      Aver.AreEqual("here is another text about the writer of", ((LJSContentNode)root.Children[2]).Content);
    }

    [Run]
    public void Case_13()
    {
      var src =
@"
  div{
   string ""con = 2 - 3 tent""
   subsection{}
   here is another text
   about the writer of
   ""'Odesskie Mansy'"" and other stuff
  }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(3, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("string con = 2 - 3 tent", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSSectionNode);
      Aver.AreEqual("subsection", ((LJSSectionNode)root.Children[1]).Name);

      Aver.IsTrue(root.Children[2] is LJSContentNode);
      Aver.AreEqual("here is another text about the writer of 'Odesskie Mansy' and other stuff", ((LJSContentNode)root.Children[2]).Content);
    }

    [Run]
    public void Case_14()
    {
      var src =
@"
  div{
   string ""con = 2 - 3 tent""
   subsection{ we shall see $""verbatim strings with many lines
line 2
line 3""
   }
   here is another text
   about the writer of
   ""'Odesskie Mansy'"" and other stuff
  }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(3, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("string con = 2 - 3 tent", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSSectionNode);
      Aver.AreEqual("subsection", ((LJSSectionNode)root.Children[1]).Name);
      Aver.AreEqual(1, ((LJSSectionNode)root.Children[1]).Children.Length);
      Aver.IsTrue(((LJSSectionNode)root.Children[1]).Children[0] is LJSContentNode);
      Aver.AreEqual(@"we shall see verbatim strings with many lines
line 2
line 3", ((LJSContentNode)((LJSSectionNode)root.Children[1]).Children[0]).Content);

      Aver.IsTrue(root.Children[2] is LJSContentNode);
      Aver.AreEqual("here is another text about the writer of 'Odesskie Mansy' and other stuff", ((LJSContentNode)root.Children[2]).Content);
    }

    [Run]
    public void Case_15()
    {
      var src =
@"
  div{
   string ""con = 2 - 3 tent""
   subsection{ we shall see /* comment not visible */ $""verbatim strings with many lines
line 2
line 3""  //another comment
line 3 ending //more comments
   }
   here is another text
   about the writer of
   ""'Odesskie Mansy'"" and other stuff
  }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(3, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("string con = 2 - 3 tent", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSSectionNode);
      Aver.AreEqual("subsection", ((LJSSectionNode)root.Children[1]).Name);
      Aver.AreEqual(1, ((LJSSectionNode)root.Children[1]).Children.Length);
      Aver.IsTrue(((LJSSectionNode)root.Children[1]).Children[0] is LJSContentNode);
      Aver.AreEqual(@"we shall see verbatim strings with many lines
line 2
line 3 line 3 ending", ((LJSContentNode)((LJSSectionNode)root.Children[1]).Children[0]).Content);

      Aver.IsTrue(root.Children[2] is LJSContentNode);
      Aver.AreEqual("here is another text about the writer of 'Odesskie Mansy' and other stuff", ((LJSContentNode)root.Children[2]).Content);
    }

    [Run]
    public void Case_16()
    {
      var src =
@"
  div{
  # script 1.1
  # script 1.2
   span{}
  # script 2
#   script 3
                                                           #script # 4 // script comment
  }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(3, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSScriptNode);
      Aver.AreEqual(" script 1.1\n script 1.2", ((LJSScriptNode)root.Children[0]).Script);

      Aver.IsTrue(root.Children[1] is LJSSectionNode);
      Aver.AreEqual("span", ((LJSSectionNode)root.Children[1]).Name);

      Aver.IsTrue(root.Children[2] is LJSScriptNode);
      Aver.AreEqual(" script 2\n   script 3\nscript # 4 // script comment", ((LJSScriptNode)root.Children[2]).Script);
    }

    [Run]
    public void Case_17()
    {
      var src =
@"
  div{
  text content
  # script content
 }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(2, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("text content", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSScriptNode);
      Aver.AreEqual(" script content", ((LJSScriptNode)root.Children[1]).Script);
    }

    [Run]
    public void Case_17_1()
    {
      var src =
@"
  div=id{
  text content
  # script content
 }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual("id", root.TranspilerPragma);
      Aver.AreEqual(2, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("text content", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSScriptNode);
      Aver.AreEqual(" script content", ((LJSScriptNode)root.Children[1]).Script);
    }

    [Run]
    public void Case_18()
    {
      var src =
@"
  div=id{
  text content
  # script content
  sect{}
 }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual("id", root.TranspilerPragma);
      Aver.AreEqual(3, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("text content", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSScriptNode);
      Aver.AreEqual(" script content", ((LJSScriptNode)root.Children[1]).Script);

      Aver.IsTrue(root.Children[2] is LJSSectionNode);
      Aver.AreEqual("sect", ((LJSSectionNode)root.Children[2]).Name);
    }

    [Run]
    public void Case_19()
    {
      var src =
@"
  div=id{
  text content
  # script content
  # and more
  sect{}
 }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual("id", root.TranspilerPragma);
      Aver.AreEqual(3, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("text content", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSScriptNode);
      Aver.AreEqual(" script content\n and more", ((LJSScriptNode)root.Children[1]).Script);

      Aver.IsTrue(root.Children[2] is LJSSectionNode);
      Aver.AreEqual("sect", ((LJSSectionNode)root.Children[2]).Name);
    }


    [Run]
    public void Case_20()
    {
      var src =
@"
  div=id{
  text content
  # script content //comment
  # and more
  sect{}
 }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual("id", root.TranspilerPragma);
      Aver.AreEqual(3, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("text content", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSScriptNode);
      Aver.AreEqual(" script content //comment\n and more", ((LJSScriptNode)root.Children[1]).Script);

      Aver.IsTrue(root.Children[2] is LJSSectionNode);
      Aver.AreEqual("sect", ((LJSSectionNode)root.Children[2]).Name);
    }

    [Run]
    public void Case_21()
    {
      var src =
@"
  div=id{
  text content
  # script content //comment
  # and more
  more text
  sect{}
 }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual("id", root.TranspilerPragma);
      Aver.AreEqual(4, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("text content", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSScriptNode);
      Aver.AreEqual(" script content //comment\n and more", ((LJSScriptNode)root.Children[1]).Script);

      Aver.IsTrue(root.Children[2] is LJSContentNode);
      Aver.AreEqual("more text", ((LJSContentNode)root.Children[2]).Content);

      Aver.IsTrue(root.Children[3] is LJSSectionNode);
      Aver.AreEqual("sect", ((LJSSectionNode)root.Children[3]).Name);
    }

    [Run]
    public void Case_22()
    {
      var src =
@"
  div=id{
  text content
  # script content(){ //comment
  # and more }
  more text
  sect{ atr1=val1 content text 'and more'}
 }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual("id", root.TranspilerPragma);
      Aver.AreEqual(4, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("text content", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSScriptNode);
      Aver.AreEqual(" script content(){ //comment\n and more }", ((LJSScriptNode)root.Children[1]).Script);

      Aver.IsTrue(root.Children[2] is LJSContentNode);
      Aver.AreEqual("more text", ((LJSContentNode)root.Children[2]).Content);

      Aver.IsTrue(root.Children[3] is LJSSectionNode);
      Aver.AreEqual("sect", ((LJSSectionNode)root.Children[3]).Name);

      Aver.AreEqual(2, ((LJSSectionNode)root.Children[3]).Children.Length);
      Aver.IsTrue(((LJSSectionNode)root.Children[3]).Children[0] is LJSAttributeNode);
      Aver.AreEqual("atr1", ((LJSAttributeNode)((LJSSectionNode)root.Children[3]).Children[0]).Name);
      Aver.AreEqual("val1", ((LJSAttributeNode)((LJSSectionNode)root.Children[3]).Children[0]).Value);

      Aver.IsTrue(((LJSSectionNode)root.Children[3]).Children[1] is LJSContentNode);
      Aver.AreEqual("content text and more", ((LJSContentNode)((LJSSectionNode)root.Children[3]).Children[1]).Content);
    }

    [Run]
    public void Case_23()
    {
      var src = "div{ single subsection{} }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual(2, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("single", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSSectionNode);
      Aver.AreEqual("subsection", ((LJSSectionNode)root.Children[1]).Name);

    }

    [Run]
    public void Case_30()
    {
      var src =
@"
  div{
   string ""con = 2 - 3 tent""
   subsection{}
   here is another text about the writer of
   span1{} span2{}
  }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.IsNull(root.TranspilerPragma);
      Aver.AreEqual(5, root.Children.Length);

      Aver.IsTrue(root.Children[0] is LJSContentNode);
      Aver.AreEqual("string con = 2 - 3 tent", ((LJSContentNode)root.Children[0]).Content);

      Aver.IsTrue(root.Children[1] is LJSSectionNode);
      Aver.AreEqual("subsection", ((LJSSectionNode)root.Children[1]).Name);

      Aver.IsTrue(root.Children[2] is LJSContentNode);
      Aver.AreEqual("here is another text about the writer of", ((LJSContentNode)root.Children[2]).Content);

      Aver.IsTrue(root.Children[3] is LJSSectionNode);
      Aver.AreEqual("span1", ((LJSSectionNode)root.Children[3]).Name);

      Aver.IsTrue(root.Children[4] is LJSSectionNode);
      Aver.AreEqual("span2", ((LJSSectionNode)root.Children[4]).Name);
    }

    [Run]
    public void UseCase_1()
    {
      var src =
@"
  div=divRoot{
   For all of these items you can hit delete:
   # for(let elm in data){//loop
   #   let d = mapData(elm);
    Name: span{class=name ""?d.Name""}
    Description: span{class='descr strong' ""?d.Description""}
   # }
  }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual("divRoot", root.TranspilerPragma);
      Aver.AreEqual(7, root.Children.Length);

      Aver.AreEqual("For all of these items you can hit delete:", ((LJSContentNode)root.Children[0]).Content);
      Aver.AreEqual(" for(let elm in data){//loop\n   let d = mapData(elm);", ((LJSScriptNode)root.Children[1]).Script);
      Aver.AreEqual("Name:", ((LJSContentNode)root.Children[2]).Content);
      Aver.AreEqual("span", ((LJSSectionNode)root.Children[3]).Name);
      Aver.AreEqual(2, ((LJSSectionNode)root.Children[3]).Children.Length);
      Aver.AreEqual("class", ((LJSAttributeNode)((LJSSectionNode)root.Children[3]).Children[0]).Name);
      Aver.AreEqual("name", ((LJSAttributeNode)((LJSSectionNode)root.Children[3]).Children[0]).Value);
      Aver.AreEqual("?d.Name", ((LJSContentNode)((LJSSectionNode)root.Children[3]).Children[1]).Content);

      Aver.AreEqual("Description:", ((LJSContentNode)root.Children[4]).Content);
      Aver.AreEqual("span", ((LJSSectionNode)root.Children[5]).Name);
      Aver.AreEqual(2, ((LJSSectionNode)root.Children[5]).Children.Length);
      Aver.AreEqual("class", ((LJSAttributeNode)((LJSSectionNode)root.Children[5]).Children[0]).Name);
      Aver.AreEqual("descr strong", ((LJSAttributeNode)((LJSSectionNode)root.Children[5]).Children[0]).Value);
      Aver.AreEqual("?d.Description", ((LJSContentNode)((LJSSectionNode)root.Children[5]).Children[1]).Content);
      Aver.AreEqual(" }", ((LJSScriptNode)root.Children[6]).Script);
    }

    [Run]
    public void UseCase_2()
    {
      var src =
@"
  div=divRoot{
   For all of these items
   you can hit delete:
   //Will use java script for now
   # for(let elm in data){//loop
   #   let d = mapData(elm);
    Name: span{class=name ""?d.Name""}
    Description: span{class='descr strong' ""?d.Description""}
   # }
         /* // old code:
         # xfor(let elm in data){//loop
         #   xlet d = mapData(elm);
          xName: span{class=name ""?d.Name""}
          xDescription: span{class='descr strong' ""?d.Description""}
         # }
        */

  }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual("divRoot", root.TranspilerPragma);
      Aver.AreEqual(7, root.Children.Length);

      Aver.AreEqual("For all of these items you can hit delete:", ((LJSContentNode)root.Children[0]).Content);
      Aver.AreEqual(" for(let elm in data){//loop\n   let d = mapData(elm);", ((LJSScriptNode)root.Children[1]).Script);
      Aver.AreEqual("Name:", ((LJSContentNode)root.Children[2]).Content);
      Aver.AreEqual("span", ((LJSSectionNode)root.Children[3]).Name);
      Aver.AreEqual(2, ((LJSSectionNode)root.Children[3]).Children.Length);
      Aver.AreEqual("class", ((LJSAttributeNode)((LJSSectionNode)root.Children[3]).Children[0]).Name);
      Aver.AreEqual("name", ((LJSAttributeNode)((LJSSectionNode)root.Children[3]).Children[0]).Value);
      Aver.AreEqual("?d.Name", ((LJSContentNode)((LJSSectionNode)root.Children[3]).Children[1]).Content);

      Aver.AreEqual("Description:", ((LJSContentNode)root.Children[4]).Content);
      Aver.AreEqual("span", ((LJSSectionNode)root.Children[5]).Name);
      Aver.AreEqual(2, ((LJSSectionNode)root.Children[5]).Children.Length);
      Aver.AreEqual("class", ((LJSAttributeNode)((LJSSectionNode)root.Children[5]).Children[0]).Name);
      Aver.AreEqual("descr strong", ((LJSAttributeNode)((LJSSectionNode)root.Children[5]).Children[0]).Value);
      Aver.AreEqual("?d.Description", ((LJSContentNode)((LJSSectionNode)root.Children[5]).Children[1]).Content);
      Aver.AreEqual(" }", ((LJSScriptNode)root.Children[6]).Script);
    }

    [Run]
    public void UseCase_3()
    {
      var src =
@"
  //Single line
  /* multi
  line*/
  div=divRoot{
   a=1
   b=2 c=3
   # js() //hs comment
   div=ggg{
    div{
      span{good}
      span{content}
    }
    e=4
    f=""this{ } =  = is atr value""
   }
  }
 ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);

      Aver.AreEqual("div", root.Name);
      Aver.AreEqual("divRoot", root.TranspilerPragma);
      Aver.AreEqual(5, root.Children.Length);

      Aver.AreEqual("a", ((LJSAttributeNode)root.Children[0]).Name);
      Aver.AreEqual("1", ((LJSAttributeNode)root.Children[0]).Value);

      Aver.AreEqual("b", ((LJSAttributeNode)root.Children[1]).Name);
      Aver.AreEqual("2", ((LJSAttributeNode)root.Children[1]).Value);

      Aver.AreEqual("c", ((LJSAttributeNode)root.Children[2]).Name);
      Aver.AreEqual("3", ((LJSAttributeNode)root.Children[2]).Value);

      Aver.AreEqual(" js() //hs comment", ((LJSScriptNode)root.Children[3]).Script);

      Aver.AreEqual("div", ((LJSSectionNode)root.Children[4]).Name);
      Aver.AreEqual("ggg", ((LJSSectionNode)root.Children[4]).TranspilerPragma);

      var div = (LJSSectionNode)((LJSSectionNode)root.Children[4]).Children[0];
      Aver.AreEqual("div", div.Name);
      Aver.AreEqual(2, div.Children.Length);
      Aver.AreEqual("span", ((LJSSectionNode)div.Children[0]).Name);
      Aver.AreEqual("span", ((LJSSectionNode)div.Children[1]).Name);

      Aver.AreEqual("good", ((LJSContentNode)((LJSSectionNode)div.Children[0]).Children[0]).Content);
      Aver.AreEqual("content", ((LJSContentNode)((LJSSectionNode)div.Children[1]).Children[0]).Content);

      Aver.AreEqual("e", ((LJSAttributeNode)((LJSSectionNode)root.Children[4]).Children[1]).Name);
      Aver.AreEqual("4", ((LJSAttributeNode)((LJSSectionNode)root.Children[4]).Children[1]).Value);

      Aver.AreEqual("f", ((LJSAttributeNode)((LJSSectionNode)root.Children[4]).Children[2]).Name);
      Aver.AreEqual("this{ } =  = is atr value", ((LJSAttributeNode)((LJSSectionNode)root.Children[4]).Children[2]).Value);
    }

    private void dump(LJSSectionNode root)
    {
      "\r\nTree Dump:".See();
      var sb = new StringBuilder();
      root.Print(sb, 0);
      sb.ToString().See();

      "    ... tree dump ended ...".See();
    }

    [Run]
    public void Baseline()//this is the document that we started LJS format design from
    {
      var src =
@"
    div=divRoot{
     class=bold-monster
     data-pub-id=""?this.id""
     data-pub-no=""?this.mapPublicationNumber()""
     data-pub-age=""?(this.age+10) + ' is his age'""
     data-attr-with-question=""??value"" // ""?value"" (single ?) will be assigned as string constant to div.""data-attr-with-question""
     on-click=""divRoot.clickHandler""

     Interestingly enough you do not need to wrap content is quotes
     because the system will concatenate all tags until it sees ""{"" or ""=""
     as one of the next tags
     ""Once upon a time there \u0434 was a girl  who liked to ""
     ""store files on disk at c:\\test\\data"" she also liked to assign ""e=2.7""
     $""you can also span
       this text many lines
       using verbatim strings""

     span{ id=spnKaka class='stronger bolder'  read books }

     and please other people by showing her Twitter message like these:
     #for(let t in this.data.twitter){
     # let d =  map(this.data.twitter[i], i => {Name: i.names[0], Age: (i.dob-DateTime.Now).Days} );

       div{
        name: span{ class=name ""?d.Name""}
        age:  span{ class=param ""?d.Age""}

        # for(let img in d.icons){
          img{ src=?img.src alt=""girl"" }
        # }

       }
     #}
    }
    ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();  //no exceptions

      var root = parser.ResultContext.ResultObject.Root;

      dump(root);
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), "ePrematureEOF")]
    public void Error_1()
    {
      var src = "";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), "eSectionNameExpected")]
    public void Error_2()
    {
      var src = "{";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), "ePrematureEOF")]
    public void Error_3()
    {
      var src = "kjkjkj";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), "eSectionNameExpected")]
    public void Error_4()
    {
      var src = "#kjkjkj";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), "ePrematureEOF")]
    public void Error_5()
    {
      var src = "root{";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), "ePrematureEOF")]
    public void Error_6()
    {
      var src = "root{ jkjklj ljlk jlkjl kjl kj klj lkj ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), "eContentPastRootSection")]
    public void Error_7()
    {
      var src = "root{ } } ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), "eSectionOrAttributeValueExpected")]
    public void Error_8()
    {
      var src = "root{ a= }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), "eSectionOrAttributeValueExpected")]
    public void Error_9()
    {
      var src = "root{ a= = = = = = { } }";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();
    }

    [Run]
    [Aver.Throws(typeof(CodeProcessorException), "eSectionOpenBraceExpected")]
    public void Error_10()
    {
      var src = " test text blah ";

      var parser = new LJSParser(new LaconfigLexer(new StringSource(src)));

      parser.Parse();
    }

  }
}
