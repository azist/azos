/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;
using System.Collections.Generic;

using Azos.Text;
using Azos.Scripting;

namespace Azos.Tests.Unit.Parsing
{

    [Runnable(TRUN.BASE)]
    public class CompilingExpressions
    {

        [Run]
        public void AlreadyCompiled()
        {
           var e = new CompilingExpressionEvaluator<object, object, object>("AlreadyCompiled", " null ");
           Aver.IsFalse(CompilingExpressionEvaluator<object, object, object>.IsScopeAlreadyCompiled("AlreadyCompiled"));
           e.Compile();
           Aver.IsTrue(CompilingExpressionEvaluator<object, object, object>.IsScopeAlreadyCompiled("AlreadyCompiled"));
           Aver.IsNull(e.Evaluate(null, null));
        }


        [Run]
        public void BasicArithmetic()
        {
           if (CompilingExpressionEvaluator<object, int, int>.IsScopeAlreadyCompiled("BasicArithmetic")) return;

           var e = new CompilingExpressionEvaluator<object, int, int>("BasicArithmetic", "2 + 2 - 4 + arg");
           Aver.AreEqual(5,  e.Evaluate(null, 5));
           Aver.AreEqual(3,  e.Evaluate(null, 3));
           Aver.AreEqual(179,  e.Evaluate(null, 179));
           Aver.AreEqual(12,  e.Evaluate(null, 12));
        }

        [Run]
        public void Strings()
        {
           if (CompilingExpressionEvaluator<string, bool, DateTime>.IsScopeAlreadyCompiled("Strings")) return;

           var e = new CompilingExpressionEvaluator<string, bool, DateTime>("Strings", " ctx.Contains(\"da\")? arg.Year==2013 : arg.Year==1900 ");
           Aver.IsTrue(e.Evaluate("da", DateTime.Parse("1/1/2013")));
           Aver.IsFalse(e.Evaluate("net", DateTime.Parse("1/1/2013")));
           Aver.IsTrue(e.Evaluate("kaka", DateTime.Parse("1/1/1900")));
        }

        [Run]
        public void InContext()
        {
           if (CompilingExpressionEvaluator<Dictionary<string, int>, int, int>.IsScopeAlreadyCompiled("InContext")) return;

           var e = new CompilingExpressionEvaluator<Dictionary<string, int>, int, int>("InContext", " ctx[\"A\"] + arg - ctx[\"B\"] ");

           var c = new Dictionary<string, int>();
           c.Add("A", 10);
           c.Add("B", -5);
           Aver.AreEqual(20,  e.Evaluate(c, 5));

           c["B"] = 100;

           Aver.AreEqual(13 - 100,  e.Evaluate(c, 3));
        }

        [Run]
        public void Many()
        {
           if (CompilingExpressionEvaluator<int, int, int>.IsScopeAlreadyCompiled("Many")) return;

           var lst = new List<CompilingExpressionEvaluator<int, int, int>>();

           for(var i=0; i<100; i++)
             lst.Add( new CompilingExpressionEvaluator<int, int, int>("Many", " ctx + arg +" + i.ToString()));

           for(var i=0; i<100; i++)
           {
             var expr = lst[i];
             Aver.AreEqual(i, expr.Evaluate(-i, i));
           }
        }


    }
}
