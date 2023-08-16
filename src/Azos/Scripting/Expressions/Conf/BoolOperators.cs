/*<FILE_LICENSE>
* Azos (A to Z Application Operating System) Framework
* The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
* See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;

namespace Azos.Scripting.Expressions.Conf
{
  public class True  : BoolTrue <IConfigSectionNode> { }
  public class False : BoolFalse<IConfigSectionNode> { }

  public class And       : BoolAnd      <IConfigSectionNode> {  }
  public class Or        : BoolOr       <IConfigSectionNode> {  }
  public class Xor       : BoolXor      <IConfigSectionNode> {  }
  public class Not       : BoolNot      <IConfigSectionNode> {  }
  public class Eq        : BoolEquals   <IConfigSectionNode> {  }
  public class NotEq     : BoolNotEquals<IConfigSectionNode> {  }
}
