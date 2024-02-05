/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using System;

using Azos.Data;

namespace Azos.Scripting.Expressions.Data
{
  public class Select : Select<ScriptCtx, Doc, Doc> { }
  public class Where  : Where<ScriptCtx, Doc> { }
  public class Any    : Any<ScriptCtx, Doc> { }
  public class Skip   : Skip<ScriptCtx, Doc> { }
  public class First  : First<ScriptCtx, Doc> { }
  public class FirstOrDefault<ScriptCtx, Doc> { }
}
