/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting;
using E = Azos.Text.Evaluator;

namespace Azos.Tests.Nub.Parsing
{
  [Runnable]
  public class EvaluatorTests
  {
    [Run]
    public void BasicArithmetic()
    {
        var e = new E("2+2-4");
        Aver.AreEqual("0",  e.Evaluate());
    }

    [Run]
    public void Precedence()
    {
        var e = new E("2+2*10");
        Aver.AreEqual("22",  e.Evaluate());
    }

    [Run]
    public void Precedence2()
    {
        var e = new E("(2+2)*10");
        Aver.AreEqual("40",  e.Evaluate());
    }

    [Run]
    public void Unary()
    {
        var e = new E("-(2+2)*10");
        Aver.AreEqual("-40",  e.Evaluate());
    }

    [Run]
    public void StringArithmetic()
    {
        var e = new E("'cold='+(-(2+2)*10)");
        Aver.AreEqual("cold=-40",  e.Evaluate());
    }

    [Run]
    public void StringArithmetic2()
    {
        var e = new E("'cold='+'hot'");
        Aver.AreEqual("cold=hot",  e.Evaluate());
    }

    [Run]
    public void StringArithmeticWithVars()
    {
        var e = new E("'cold='+(-(2+2)*x)");
        e.OnIdentifierLookup += new Text.IdentifierLookup((ident)=>ident=="x"?"100":ident);
        Aver.AreEqual("cold=-400",  e.Evaluate());
    }

    [Run]
    public void StringArithmeticWithVars_Lambda()
    {
        var e = new E("'cold='+(-(2+2)*x)");
        Aver.AreEqual("cold=-400",  e.Evaluate((ident)=>ident=="x"?"100":ident));
    }

    [Run]
    public void Constants()
    {
        var e = new E("Pi*2");
        Aver.AreEqual("6.2831",  e.Evaluate().Substring(0, 6));
    }

    [Run]
    public void Conditional()
    {
        var e = new E("'less:'+(?10<20;'yes';'no')");
        Aver.AreEqual("less:yes",  e.Evaluate());
    }

    [Run]
    public void ConditionalWithVars()
    {
        var e = new E("'less:'+(?x<y;'yes';'no')");

        var x = "10";
        var y = "20";
        e.OnIdentifierLookup += new Text.IdentifierLookup((ident)=>ident=="x"?x:ident=="y"?y:ident);

        Aver.AreEqual("less:yes",  e.Evaluate());

        y = "0";
        Aver.AreEqual("less:no",  e.Evaluate());
    }

    [Run]
    public void ConditionalWithVars_Lambda()
    {
        var e = new E("'less:'+(?x<y;'yes';'no')");

        var x = "10";
        var y = "20";

        Aver.AreEqual("less:yes",  e.Evaluate((ident)=>ident=="x"?x:ident=="y"?y:ident));

        y = "0";
        Aver.AreEqual("less:no",  e.Evaluate((ident)=>ident=="x"?x:ident=="y"?y:ident));
    }

  }
}


