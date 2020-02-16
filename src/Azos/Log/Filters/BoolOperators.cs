/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Scripting.Expressions;

namespace Azos.Log.Filters
{
  public class And       : BoolAndOperator       <Message> { }
  public class Or        : BoolOrOperator        <Message> { }
  public class Xor       : BoolXorOperator       <Message> { }
  public class Not       : BoolNotOperator       <Message> { }
  public class Equals    : BoolEqualsOperator    <Message> { }
  public class NotEquals : BoolNotEqualsOperator <Message> { }
}
