
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
