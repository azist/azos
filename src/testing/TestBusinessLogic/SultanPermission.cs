/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Security;

namespace TestBusinessLogic
{
    /// <summary>
    /// If I were sultan, i'd have 3 wifes
    /// </summary>
    public sealed class SultanPermission : TypedPermission
    {
       public SultanPermission(int level) : base(level)
       {
       }
    }
}
