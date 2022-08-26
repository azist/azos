/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

namespace Azos.Sky.Metabase
{
    /// <summary>
    /// Designates types of reachability in the named network
    /// </summary>
    public enum NetworkScope
    {
       /// <summary>
       /// Everything accessible from everywhere
       /// </summary>
       Any,

       /// <summary>
       /// Only accessible within the same Network Operation Center
       /// </summary>
       NOC,

       /// <summary>
       /// Only accessible within the same-named group. Groups are properties of every host. Groups are defined in 'networks' root file
       /// </summary>
       Group,

       /// <summary>
       /// Only accessible within the same named group within the same Network Operation Center
       /// </summary>
       NOCGroup
    }
}
