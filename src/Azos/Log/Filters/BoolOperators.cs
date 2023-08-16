/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting.Expressions;

namespace Azos.Log.Filters
{
  public class True  : BoolTrue<Message>  { }
  public class False : BoolFalse<Message> { }

  public class And       : BoolAnd       <Message> { }
  public class Or        : BoolOr        <Message> { }
  public class Xor       : BoolXor       <Message> { }
  public class Not       : BoolNot       <Message> { }
  public class Equals    : BoolEquals    <Message> { }
  public class NotEquals : BoolNotEquals <Message> { }
}
