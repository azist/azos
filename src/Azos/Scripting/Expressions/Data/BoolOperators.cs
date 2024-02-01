/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/


namespace Azos.Scripting.Expressions.Data
{
  public class True  : BoolTrue <ScriptCtx> { }
  public class False : BoolFalse<ScriptCtx> { }

  public class And       : BoolAnd      <ScriptCtx> {  }
  public class Or        : BoolOr       <ScriptCtx> {  }
  public class Xor       : BoolXor      <ScriptCtx> {  }
  public class Not       : BoolNot      <ScriptCtx> {  }
  public class Eq        : BoolObjectEquals   <ScriptCtx> {  }
  public class NotEq     : BoolObjectNotEquals<ScriptCtx> {  }
}
