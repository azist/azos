/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Conf;
using Azos.Data.Access;

using Azos.Sky.Apps;
using Azos.Sky.Identification;
using Azos.Sky.Metabase;

namespace Azos.Sky
{
  /// <summary>
  /// Provides a shortcut access to app-global Sky context
  /// </summary>
  public static class SkySystem
  {
    private static BuildInformation s_CoreBuildInfo;

    /// <summary>
    /// Returns BuildInformation object for the core Sky assembly
    /// </summary>
    public static BuildInformation CoreBuildInfo
    {
      get
      {
        //multithreading: 2nd copy is ok
        if (s_CoreBuildInfo == null)
          s_CoreBuildInfo = new BuildInformation(typeof(SkySystem).Assembly);

        return s_CoreBuildInfo;
      }
    }

    private static string s_MetabaseApplicationName;

    /// <summary>
    /// Every Sky application MUST ASSIGN THIS property at its entry point ONCE. Example: void Main(string[]args){ SkySystem.MetabaseApplicationName = "MyApp1";...
    /// </summary>
    public static string MetabaseApplicationName
    {
      get { return s_MetabaseApplicationName; }
      set
      {
        if (s_MetabaseApplicationName != null || value.IsNullOrWhiteSpace())
          throw new SkyException(StringConsts.METABASE_APP_NAME_ASSIGNMENT_ERROR);
        s_MetabaseApplicationName = value;
      }
    }

  }
}
