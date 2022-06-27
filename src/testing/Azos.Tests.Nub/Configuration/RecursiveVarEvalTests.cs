/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Conf;
using Azos.Scripting;

namespace Azos.Tests.Nub.Configuration
{
  [Runnable]
  public class RecursiveVarEvalTests
  {
    [Run]
    public void EvaluateVarsExtension_1()
    {
      var vars = new Vars();
      vars["a"] = "1";
      vars["b"] = "2";
      vars["c"] = "$(~a)";
      vars["d"] = "$(~a)$(~b)";
      vars["e"] = "$(~d)";

      Aver.AreEqual("Value is: 1", "Value is: $(~a)".EvaluateVars(vars));
      Aver.AreEqual("1 Value is: 1", "$(~a) Value is: $(~a)".EvaluateVars(vars));
      Aver.AreEqual("1 Value is: 2", "$(~a) Value is: $(~b)".EvaluateVars(vars));
      Aver.AreEqual("1 Value is: 1", "$(~c) Value is: $(~a)".EvaluateVars(vars));
      Aver.AreEqual("$(~a) Value is: 1", "$(~c) Value is: $(~a)".EvaluateVars(vars, recurse: false));

      Aver.AreEqual("v = 12", "v = $(~e)".EvaluateVars(vars));
      Aver.AreEqual("v = $(~d)", "v = $(~e)".EvaluateVars(vars, recurse: false));
    }

    [Run]
    public void EvaluateVarsExtension_2()
    {
      var vars = new Vars();
      vars["long-var-name-a"] = "1";
      vars["long-var-name-b"] = "2";
      vars["long-var-name-c"] = "$(~long-var-name-a)";
      vars["long-var-name-d"] = "$(~long-var-name-a)$(~long-var-name-b)";
      vars["long-var-name-e"] = "$(~long-var-name-d)";

      Aver.AreEqual("Value is: 1", "Value is: $(~long-var-name-a)".EvaluateVars(vars));
      Aver.AreEqual("1 Value is: 1", "$(~long-var-name-a) Value is: $(~long-var-name-a)".EvaluateVars(vars));
      Aver.AreEqual("1 Value is: 2", "$(~long-var-name-a) Value is: $(~long-var-name-b)".EvaluateVars(vars));
      Aver.AreEqual("1 Value is: 1", "$(~long-var-name-c) Value is: $(~long-var-name-a)".EvaluateVars(vars));
      Aver.AreEqual("$(~long-var-name-a) Value is: 1", "$(~long-var-name-c) Value is: $(~long-var-name-a)".EvaluateVars(vars, recurse: false));

      Aver.AreEqual("v = 12", "v = $(~long-var-name-e)".EvaluateVars(vars));
      Aver.AreEqual("v = $(~long-var-name-d)", "v = $(~long-var-name-e)".EvaluateVars(vars, recurse: false));
    }

    [Run]
    public void EvaluateVarsExtension_3()
    {
      var vars = new Vars();
      vars["a"] = "long-var-value-1";
      vars["b"] = "long-var-value-2";
      vars["c"] = "long-var-value-$(~a)";
      vars["d"] = "long-var-value-$(~a)$(~b)";
      vars["e"] = "long-var-value-$(~d)";

      Aver.AreEqual("Value is: long-var-value-1", "Value is: $(~a)".EvaluateVars(vars));
      Aver.AreEqual("long-var-value-1 Value is: long-var-value-1", "$(~a) Value is: $(~a)".EvaluateVars(vars));
      Aver.AreEqual("long-var-value-1 Value is: long-var-value-2", "$(~a) Value is: $(~b)".EvaluateVars(vars));
      Aver.AreEqual("long-var-value-long-var-value-1 Value is: long-var-value-1", "$(~c) Value is: $(~a)".EvaluateVars(vars));
      Aver.AreEqual("long-var-value-$(~a) Value is: long-var-value-1", "$(~c) Value is: $(~a)".EvaluateVars(vars, recurse: false));

      Aver.AreEqual("v = long-var-value-long-var-value-long-var-value-1long-var-value-2", "v = $(~e)".EvaluateVars(vars));
      Aver.AreEqual("v = long-var-value-$(~d)", "v = $(~e)".EvaluateVars(vars, recurse: false));
    }


    [Run]
    public void EvaluateVarsExtension_4()
    {
      var vars = new Vars();
      vars["long-var-name-a"] = "long-var-value-1";
      vars["long-var-name-b"] = "long-var-value-2";
      vars["long-var-name-c"] = "long-var-value-$(~long-var-name-a)";
      vars["long-var-name-d"] = "long-var-value-$(~long-var-name-a)$(~long-var-name-b)";
      vars["long-var-name-e"] = "long-var-value-$(~long-var-name-d)";

      Aver.AreEqual("Value is: long-var-value-1", "Value is: $(~long-var-name-a)".EvaluateVars(vars));
      Aver.AreEqual("long-var-value-1 Value is: long-var-value-1", "$(~long-var-name-a) Value is: $(~long-var-name-a)".EvaluateVars(vars));
      Aver.AreEqual("long-var-value-1 Value is: long-var-value-2", "$(~long-var-name-a) Value is: $(~long-var-name-b)".EvaluateVars(vars));
      Aver.AreEqual("long-var-value-long-var-value-1 Value is: long-var-value-1", "$(~long-var-name-c) Value is: $(~long-var-name-a)".EvaluateVars(vars));
      Aver.AreEqual("long-var-value-$(~long-var-name-a) Value is: long-var-value-1", "$(~long-var-name-c) Value is: $(~long-var-name-a)".EvaluateVars(vars, recurse: false));

      Aver.AreEqual("v = long-var-value-long-var-value-long-var-value-1long-var-value-2", "v = $(~long-var-name-e)".EvaluateVars(vars));
      Aver.AreEqual("v = long-var-value-$(~long-var-name-d)", "v = $(~long-var-name-e)".EvaluateVars(vars, recurse: false));
    }

    [Run]
    public void EvaluateVarsExtension_5()
    {
      var vars = new Vars();
      vars["a"] = "";
      vars["b"] = " ";
      vars["c"] = null;
      vars["d"] = "$(~a).$(~b).$(~c)";
      vars["e"] = "$(~d)";
      vars["f01234567890123456789"] = "$(~e)";
      vars["zzz01234567890123456789"] = "$(~f01234567890123456789)";

      vars["v1"] = "value 1";
      vars["v2"] = "2 value";

      vars["x1"] = "$(~v1) some other text $(~v2) and more text $(~v1)";

      Aver.AreEqual("Value is: ", "Value is: $(~a)".EvaluateVars(vars));
      Aver.AreEqual("Value is:  ", "Value is: $(~b)".EvaluateVars(vars));
      Aver.AreEqual("Value is: ", "Value is: $(~c)".EvaluateVars(vars));
      Aver.AreEqual("Value is: ", "Value is: $(~zzzz)".EvaluateVars(vars));

      Aver.AreEqual("Value is: . .end", "Value is: $(~d)end".EvaluateVars(vars));
      Aver.AreEqual("Value is: . .end", "Value is: $(~e)end".EvaluateVars(vars));
      Aver.AreEqual("Value is: . .end", "Value is: $(~f01234567890123456789)end".EvaluateVars(vars));
      Aver.AreEqual("Value is: . .end", "Value is: $(~zzz01234567890123456789)end".EvaluateVars(vars));

      Aver.AreEqual("Value is: value 1 some other text 2 value and more text value 1end", "Value is: $(~x1)end".EvaluateVars(vars));

      Aver.AreEqual("Value is: ", "Value is: $(~a)".EvaluateVars(vars, recurse: false));
      Aver.AreEqual("Value is:  ", "Value is: $(~b)".EvaluateVars(vars, recurse: false));
      Aver.AreEqual("Value is: ", "Value is: $(~c)".EvaluateVars(vars, recurse: false));
      Aver.AreEqual("Value is: ", "Value is: $(~zzzz)".EvaluateVars(vars, recurse: false));
      Aver.AreEqual("Value is: $(~a).$(~b).$(~c)end", "Value is: $(~d)end".EvaluateVars(vars, recurse: false));
      Aver.AreEqual("Value is: $(~d)end", "Value is: $(~e)end".EvaluateVars(vars, recurse: false));
      Aver.AreEqual("Value is: $(~e)end", "Value is: $(~f01234567890123456789)end".EvaluateVars(vars, recurse: false));
      Aver.AreEqual("Value is: $(~f01234567890123456789)end", "Value is: $(~zzz01234567890123456789)end".EvaluateVars(vars, recurse: false));
      Aver.AreEqual("Value is: $(~v1) some other text $(~v2) and more text $(~v1)end", "Value is: $(~x1)end".EvaluateVars(vars, recurse: false));
    }

    [Run]
    public void EvaluateVarsExtension_6()
    {
      var vars = new Vars();
      vars["a"] = "a";
      vars["b"] = "b";
      vars["c"] = "/c";
      vars["d"] = "$(@~a)$(@~b)$(@~c)";

      Aver.AreEqual("path={0}a{0}b{0}c".Args(System.IO.Path.DirectorySeparatorChar), "path=$(@~a)$(@~b)$(@~c)".EvaluateVars(vars));
      Aver.AreEqual("path={0}a{0}b{0}c".Args(System.IO.Path.DirectorySeparatorChar), "path=$(~d)".EvaluateVars(vars));

      Aver.AreEqual("path={0}a{0}b{0}c".Args(System.IO.Path.DirectorySeparatorChar), "path=$(@~a)$(@~b)$(@~c)".EvaluateVars(vars, recurse: false));
      Aver.AreEqual("path=$(@~a)$(@~b)$(@~c)", "path=$(~d)".EvaluateVars(vars, recurse: false));
    }

    [Run]
    public void EvaluateVarsExtension_7()
    {
      var vars = new Vars();
      vars["a"] = "a";
      vars["b"] = "b";
      vars["c"] = "/c";
      vars["d"] = "$(@~a)$(@~b)$(@~c)$(~a)$(~b)$(~c)$(~zzzzz)$(~a)$(@~a)";

      Aver.AreEqual("path={0}a{0}b{0}c{0}ab/ca{0}a".Args(System.IO.Path.DirectorySeparatorChar), "path=$(~d)".EvaluateVars(vars));

      Aver.AreEqual("path=$(@~a)$(@~b)$(@~c)$(~a)$(~b)$(~c)$(~zzzzz)$(~a)$(@~a)", "path=$(~d)".EvaluateVars(vars, recurse: false));
    }
  }
}
