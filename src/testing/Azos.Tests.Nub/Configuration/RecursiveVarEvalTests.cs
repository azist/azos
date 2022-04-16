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
  }
}
