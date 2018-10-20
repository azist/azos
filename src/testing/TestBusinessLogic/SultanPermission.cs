
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
