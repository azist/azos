/*<FILE_LICENSE>
 * Azos (A to Z Application Operating System) Framework
 * The A to Z Foundation (a.k.a. Azist) licenses this file to you under the MIT license.
 * See the LICENSE file in the project root for more information.
</FILE_LICENSE>*/

using Azos.Sky;

namespace Azos.Tests.Unit.Sky
{
  internal static class TestSources
  {

    public static string RPATH
     {
       get
       {
         var vname = Azos.Apps.BootConfLoader.ENV_VAR_METABASE_FS_ROOT + "_UTEST";
         var result = System.Environment.GetEnvironmentVariable(vname);
         if (result.IsNullOrWhiteSpace())
           throw new SkyException(
@"

  This machine is not setup for unit testing.

  The variable:   `{0}`   must point to test metabase root

".Args(vname));

         return result;
       }
     }


     public static string THIS_HOST
     {
       get
       {
         var vname = Azos.Apps.BootConfLoader.ENV_VAR_HOST_NAME + "_UTEST";
         var result = System.Environment.GetEnvironmentVariable(vname);
         if (result.IsNullOrWhiteSpace())
           throw new SkyException(
@"

  This machine is not setup for unit testing.

  The variable:   `{0}`   must contain a valid metabase host name

".Args(vname));

         return result;
       }
     }

     public const int PARALLEL_LOOP_TO = 250000;
  }
}
