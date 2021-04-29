/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Security;

namespace Azos.Tests.Nub.Security
{
  public class TeztPermissionA : TypedPermission
  {
    public TeztPermissionA(int level) : base(level){ }
  }

  public class TeztPermissionB : TypedPermission
  {
    public TeztPermissionB(int level) : base(level) { }
  }

  public class TeztPermissionC : TypedPermission
  {
    public TeztPermissionC(int level) : base(level) { }
  }
}
